using System;
using SharpDX;
using SharpDX.Toolkit.Graphics;

namespace factor10.VisionThing.Effects
{
    public class VisionEffect : IVEffect, IDisposable
    {
        public GraphicsDevice GraphicsDevice { get; private set; }
        public Effect Effect { get; private set; }

        public readonly string Name;

        protected readonly EffectParameter _epTextureSampler;

        protected readonly EffectParameter _epWorld;
        protected readonly EffectParameter _epWorldInverseTranspose;
        protected readonly EffectParameter _epView;
        protected readonly EffectParameter _epProjection;
        protected readonly EffectParameter _epCameraPosition;
        protected readonly EffectParameter _epClipPlane;
        protected readonly EffectParameter _epSunlightDirection;
        protected readonly EffectParameter _epTexture;
        protected readonly EffectParameter _epDiffuseColor;

        protected readonly EffectParameter _epDoShadowMapping;
        protected readonly EffectParameter _epShadowMap;
        protected readonly EffectParameter _epShadowViewProjection;
        protected readonly EffectParameter _epShadowFarPlane;
        protected readonly EffectParameter _epShadowMult;

        protected readonly EffectTechnique _techStandard;
        protected readonly EffectTechnique _techClipPlane;
        protected readonly EffectTechnique _techDepthMap;

        public VisionEffect(Effect effect, SamplerState samplerState = null)
        {
            GraphicsDevice = effect.GraphicsDevice;
            Effect = effect;
            Name = effect.Name;

            _epTextureSampler = effect.Parameters["TextureSampler"];
            if(_epTextureSampler!=null)
                Sampler = samplerState ?? GraphicsDevice.SamplerStates.LinearWrap;

            _epWorld = effect.Parameters["World"];
            _epWorldInverseTranspose = effect.Parameters["WorldInverseTranspose"];
            _epView = effect.Parameters["View"];
            _epProjection = effect.Parameters["Projection"];
            _epCameraPosition = effect.Parameters["CameraPosition"];
            _epClipPlane = effect.Parameters["ClipPlane"];
            _epSunlightDirection = effect.Parameters["SunlightDirection"];
            _epTexture = effect.Parameters["Texture"];
            _epDiffuseColor = effect.Parameters["DiffuseColor"];

            _epDoShadowMapping = effect.Parameters["DoShadowMapping"];
            _epShadowMap = effect.Parameters["ShadowMap"];
            _epShadowViewProjection = effect.Parameters["ShadowViewProjection"];
            _epShadowFarPlane = effect.Parameters["ShadowFarPlane"];
            _epShadowMult = effect.Parameters["ShadowMult"];

            _techStandard = effect.Techniques["TechStandard"];
            _techClipPlane = effect.Techniques["TechClipPlane"];
            _techDepthMap = effect.Techniques["TechDepthMap"];

            //Debug.Assert( _epView != null );
            //Debug.Assert(_techDepthMap != null);

            if ( _epSunlightDirection != null)
                _epSunlightDirection.SetValue(VisionContent.SunlightDirection);
        }

        public Matrix World
        {
            get { return _epWorld.GetMatrix(); }
            set
            {
                _epWorld.SetValue(value);
                if (_epWorldInverseTranspose != null)
                {
                    value.Invert();
                    value.Transpose();
                    _epWorldInverseTranspose.SetValue(value);
                }
            }
        }

        public Matrix View
        {
            get { return _epView.GetMatrix(); }
            set { _epView.SetValue(value); }
        }

        public Matrix Projection
        {
            get { return _epProjection.GetMatrix(); }
            set { _epProjection.SetValue(value); }
        }

        public Vector3 CameraPosition
        {
            get { return _epCameraPosition.GetValue<Vector3>(); }
            set
            {
                if ( _epCameraPosition != null)
                    _epCameraPosition.SetValue(value);
            }
        }

        public Vector3 SunlightDirection
        {
            get { return _epSunlightDirection.GetValue<Vector3>(); }
            set
            {
                if (_epSunlightDirection != null)
                    _epSunlightDirection.SetValue(value);
            }
        }

        public Vector4 DiffuseColor
        {
            get { return _epDiffuseColor.GetValue<Vector4>(); }
            set { _epDiffuseColor.SetValue(value); }
        }

        public Vector4? ClipPlane
        {
            set
            {
                if ( _epClipPlane== null )
                    return;
                if (value.HasValue)
                {
                    _epClipPlane.SetValue(value.Value);
                    Effect.CurrentTechnique = _techClipPlane;
                }
                else
                    Effect.CurrentTechnique = _techStandard;
            }
        }

        public SamplerState Sampler
        {
            get { return _epTextureSampler.GetResource<SamplerState>(); }
            set { _epTextureSampler.SetResource(value); }
        }

        public Texture2DBase Texture
        {
            get { return _epTexture.GetResource<Texture2D>(); }
            set { _epTexture.SetResource(value); }
        }

        public void Apply()
        {
            Effect.CurrentTechnique.Passes[0].Apply();
        }

        public EffectParameterCollection Parameters
        {
            get { return Effect.Parameters; }
        }

        public void SetShadowMapping(ShadowMap shadow)
        {
            if (_epDoShadowMapping == null)
                return;  //this effect cannot do shadow mapping

            if (shadow != null)
            {
                _epDoShadowMapping.SetValue(true);
                _epShadowMap.SetResource(shadow.ShadowDepthTarget);
                _epShadowViewProjection.SetValue(shadow.Camera.View*shadow.Camera.Projection);
                _epShadowMult.SetValue(shadow.ShadowMult);
            }
            else
                _epDoShadowMapping.SetValue(false);
        }

        public void SetTechnique(DrawingReason drawingReason )
        {
            switch ( drawingReason )
            {
                case DrawingReason.Normal:
                    Effect.CurrentTechnique = _techStandard;
                    break;
                case DrawingReason.ReflectionMap:
                    Effect.CurrentTechnique = _techClipPlane;
                    break;
                case DrawingReason.ShadowDepthMap:
                    Effect.CurrentTechnique = _techDepthMap;
                    break;
            }
        }

        public void ForEachPass(Action action)
        {
            foreach (var pass in Effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                action();
            }
        }

        public void Dispose()
        {
            Effect.Dispose();
        }

    }

}
