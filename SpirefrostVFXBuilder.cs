using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Policy;
using Deadpan.Enums.Engine.Components.Modding;
using UnityEngine;
using WildfrostHopeMod.Utils;
using WildfrostHopeMod.VFX;
using Extensions = Deadpan.Enums.Engine.Components.Modding.Extensions;

namespace Spirefrost
{
    internal class SpirefrostVFXBuilder
    {
        internal enum InterpolationType
        {
            Constant,
            Linear,
            Smooth
        }

        private static class Interpolator
        {
            internal static float Interpolate(float from, float to, float t, InterpolationType type)
            {
                switch (type)
                {
                    case InterpolationType.Linear:
                        return Mathf.Lerp(from, to, t);
                    case InterpolationType.Smooth:
                        return Mathf.SmoothStep(from, to, t);
                    default:
                        return from;
                }
            }
        }
        
        internal SpirefrostVFXBuilder(WildfrostMod mod, string path, string name = null)
        {
            _name = Extensions.PrefixGUID(name ?? Path.GetFileNameWithoutExtension(path), mod);
            _tex = mod.ImagePath(path).ToTex();
        }

        private readonly string _name;
        private readonly Texture2D _tex;
        private float _duration = 1f;
        private GIFLoader.PlayType _playType = GIFLoader.PlayType.applyEffect;
        private ParticleSystem.MinMaxGradient _color;
        private bool _hasColor;
        private ParticleSystem.MinMaxCurve _sizeX;
        private ParticleSystem.MinMaxCurve _sizeY;
        private ParticleSystem.MinMaxCurve _sizeZ;
        private bool _hasSize;
        private bool _hasDifferentSizes;
        private ParticleSystem.MinMaxCurve _velX;
        private ParticleSystem.MinMaxCurve _velY;
        private ParticleSystem.MinMaxCurve _velZ;
        private bool _hasVelocity;
        private ParticleSystem.MinMaxCurve _rotX;
        private ParticleSystem.MinMaxCurve _rotY;
        private ParticleSystem.MinMaxCurve _rotZ;
        private bool _hasRotation;
        private ParticleSystem.MinMaxCurve _initialRotX;
        private ParticleSystem.MinMaxCurve _initialRotY;
        private ParticleSystem.MinMaxCurve _initialRotZ;
        private bool _hasInitialRotation;
        private ParticleSystem.MinMaxCurve _gravity;
        private bool _hasGravity;

        internal SpirefrostVFXBuilder WithDuration(float duration)
        {
            _duration = duration;
            return this;
        }

        internal SpirefrostVFXBuilder WithPlayType(GIFLoader.PlayType playType)
        {
            _playType = playType;
            return this;
        }

        internal SpirefrostVFXBuilder WithColor(Color color)
        {
            _color = new ParticleSystem.MinMaxGradient(color);
            _hasColor = true;
            return this;
        }

        internal SpirefrostVFXBuilder WithColorGradient(Color from, Color to)
        {
            if (from == to)
            {
                return WithColor(from);
            }
            Gradient grad = new Gradient
            {
                colorKeys = new GradientColorKey[]
                {
                    new GradientColorKey(from, 0),
                    new GradientColorKey(to, 1)
                },
                alphaKeys = new GradientAlphaKey[]
                {
                    new GradientAlphaKey(from.a, 0),
                    new GradientAlphaKey(to.a, 1)
                },
                mode = GradientMode.Blend
            };
            _color = new ParticleSystem.MinMaxGradient(grad);
            _hasColor = true;
            return this;
        }

        internal SpirefrostVFXBuilder WithVelocity(Vector3 vel)
        {
            _velX = new ParticleSystem.MinMaxCurve(1, AnimationCurve.Constant(0, 1, vel.x));
            _velY = new ParticleSystem.MinMaxCurve(1, AnimationCurve.Constant(0, 1, vel.y));
            _velZ = new ParticleSystem.MinMaxCurve(1, AnimationCurve.Constant(0, 1, vel.z));
            _hasVelocity = true;
            return this;
        }

        internal SpirefrostVFXBuilder WithVelocityGradient(Vector3 from, Vector3 to, bool smooth = false)
        {
            if (from.x == from.y && from.y == from.z && to.x == to.y && to.y == to.z)
            {
                return WithVelocity(from);
            }
            if (smooth)
            {
                _velX = new ParticleSystem.MinMaxCurve(1, AnimationCurve.EaseInOut(0, from.x, 1, to.x));
                _velY = new ParticleSystem.MinMaxCurve(1, AnimationCurve.EaseInOut(0, from.y, 1, to.y));
                _velZ = new ParticleSystem.MinMaxCurve(1, AnimationCurve.EaseInOut(0, from.z, 1, to.z));
            } 
            else
            {
                _velX = new ParticleSystem.MinMaxCurve(1, AnimationCurve.Linear(0, from.x, 1, to.x));
                _velY = new ParticleSystem.MinMaxCurve(1, AnimationCurve.Linear(0, from.y, 1, to.y));
                _velZ = new ParticleSystem.MinMaxCurve(1, AnimationCurve.Linear(0, from.z, 1, to.z));
            }
            _hasVelocity = true;
            return this;
        }

        internal SpirefrostVFXBuilder WithRandomVelocity(Vector3 from, Vector3 to)
        {
            _velX = new ParticleSystem.MinMaxCurve(from.x, to.x);
            _velY = new ParticleSystem.MinMaxCurve(from.y, to.y);
            _velZ = new ParticleSystem.MinMaxCurve(from.z, to.z);
            _hasVelocity = true;
            return this;
        }

        internal SpirefrostVFXBuilder WithSize(float size)
        {
            _sizeX = new ParticleSystem.MinMaxCurve(1, AnimationCurve.Constant(0, 1, size));
            _hasSize = true;
            return this;
        }

        internal SpirefrostVFXBuilder WithSize(Vector3 sizes)
        {
            if (sizes.x == sizes.y && sizes.y == sizes.z)
            {
                return WithSize(sizes.x);
            }
            _sizeX = new ParticleSystem.MinMaxCurve(1, AnimationCurve.Constant(0, 1, sizes.x));
            _sizeY = new ParticleSystem.MinMaxCurve(1, AnimationCurve.Constant(0, 1, sizes.y));
            _sizeZ = new ParticleSystem.MinMaxCurve(1, AnimationCurve.Constant(0, 1, sizes.z));
            _hasSize = true;
            _hasDifferentSizes = true;
            return this;
        }

        internal SpirefrostVFXBuilder WithSizeGradient(float from, float to, bool smooth = false)
        {
            if (from == to)
            {
                return WithSize(from);
            }
            if (smooth)
            {
                _sizeX = new ParticleSystem.MinMaxCurve(1, AnimationCurve.EaseInOut(0, from, 1, to));
            }
            else
            {
                _sizeX = new ParticleSystem.MinMaxCurve(1, AnimationCurve.Linear(0, from, 1, to));
            }
            _hasSize = true;
            return this;
        }

        internal SpirefrostVFXBuilder WithSizeGradient(Vector3 from, Vector3 to,  bool smooth = false)
        {
            if (from.x == from.y && from.y == from.z && to.x == to.y && to.y == to.z)
            {
                return WithSizeGradient(from.x, to.x);
            }
            if (smooth)
            {
                _sizeX = new ParticleSystem.MinMaxCurve(1, AnimationCurve.EaseInOut(0, from.x, 1, to.x));
                _sizeY = new ParticleSystem.MinMaxCurve(1, AnimationCurve.EaseInOut(0, from.y, 1, to.y));
                _sizeZ = new ParticleSystem.MinMaxCurve(1, AnimationCurve.EaseInOut(0, from.z, 1, to.z));
            }
            else
            {
                _sizeX = new ParticleSystem.MinMaxCurve(1, AnimationCurve.Linear(0, from.x, 1, to.x));
                _sizeY = new ParticleSystem.MinMaxCurve(1, AnimationCurve.Linear(0, from.y, 1, to.y));
                _sizeZ = new ParticleSystem.MinMaxCurve(1, AnimationCurve.Linear(0, from.z, 1, to.z));
            }
            _hasSize = true;
            _hasDifferentSizes = true;
            return this;
        }

        internal SpirefrostVFXBuilder WithRotation(Vector3 rotation)
        {
            _rotX = new ParticleSystem.MinMaxCurve(1, AnimationCurve.Constant(0, 1, rotation.x));
            _rotY = new ParticleSystem.MinMaxCurve(1, AnimationCurve.Constant(0, 1, rotation.y));
            _rotZ = new ParticleSystem.MinMaxCurve(1, AnimationCurve.Constant(0, 1, rotation.z));
            _hasRotation = true;
            return this;
        }

        internal SpirefrostVFXBuilder WithRotationGradient(Vector3 from, Vector3 to, bool smooth = false)
        {
            if (from == to)
            {
                return WithRotation(from);
            }
            if (smooth)
            {
                _rotX = new ParticleSystem.MinMaxCurve(1, AnimationCurve.EaseInOut(0, from.x, 1, to.x));
                _rotY = new ParticleSystem.MinMaxCurve(1, AnimationCurve.EaseInOut(0, from.y, 1, to.y));
                _rotZ = new ParticleSystem.MinMaxCurve(1, AnimationCurve.EaseInOut(0, from.z, 1, to.z));
            }
            else
            {
                _rotX = new ParticleSystem.MinMaxCurve(1, AnimationCurve.Linear(0, from.x, 1, to.x));
                _rotY = new ParticleSystem.MinMaxCurve(1, AnimationCurve.Linear(0, from.y, 1, to.y));
                _rotZ = new ParticleSystem.MinMaxCurve(1, AnimationCurve.Linear(0, from.z, 1, to.z));
            }
            _hasRotation = true;
            return this;
        }

        internal SpirefrostVFXBuilder WithRandomRotation(Vector3 from, Vector3 to)
        {
            _rotX = new ParticleSystem.MinMaxCurve(from.x, to.x);
            _rotY = new ParticleSystem.MinMaxCurve(from.y, to.y);
            _rotZ = new ParticleSystem.MinMaxCurve(from.z, to.z);
            _hasRotation = true;
            return this;
        }

        internal SpirefrostVFXBuilder WithInitialRotation(Vector3 rotation)
        {
            _initialRotX = new ParticleSystem.MinMaxCurve(1, AnimationCurve.Constant(0, 1, rotation.x));
            _initialRotY = new ParticleSystem.MinMaxCurve(1, AnimationCurve.Constant(0, 1, rotation.y));
            _initialRotZ = new ParticleSystem.MinMaxCurve(1, AnimationCurve.Constant(0, 1, rotation.z));
            _hasInitialRotation = true;
            return this;
        }

        internal SpirefrostVFXBuilder WithRandomInitialRotation(Vector3 from, Vector3 to)
        {
            _initialRotX = new ParticleSystem.MinMaxCurve(from.x, to.x);
            _initialRotY = new ParticleSystem.MinMaxCurve(from.y, to.y);
            _initialRotZ = new ParticleSystem.MinMaxCurve(from.z, to.z);
            _hasInitialRotation = true;
            return this;
        }

        internal SpirefrostVFXBuilder WithGravity(float gravity)
        {
            _gravity = new ParticleSystem.MinMaxCurve(1, AnimationCurve.Constant(0, 1, gravity));
            _hasGravity = true;
            return this;
        }

        internal SpirefrostVFXBuilder WithGravityGradient(float from, float to, bool smooth = false)
        {
            if (smooth)
            {
                _gravity = new ParticleSystem.MinMaxCurve(1, AnimationCurve.EaseInOut(0, from, 1, to));
            }
            else
            {
                _gravity = new ParticleSystem.MinMaxCurve(1, AnimationCurve.Linear(0, from, 1, to));
            }
            _hasGravity = true;
            return this;
        }

        internal SpirefrostVFXBuilder WithRandomGravity(float from, float to)
        {
            _gravity = new ParticleSystem.MinMaxCurve(from, to);
            _hasGravity = true;
            return this;
        }

        internal GameObject Build()
        {
            bool destroyOnEnd = _playType == GIFLoader.PlayType.applyEffect || _playType == GIFLoader.PlayType.damageEffect;
            int loops = _playType == GIFLoader.PlayType.loopingAnimation ? -1 : 1;
            GameObject prefab = MakePrefab(_name, _tex, destroyOnEnd);
            if (VFXMod.parent)
            {
                prefab.transform.SetParent(VFXMod.parent);
            }
            else
            {
                UnityEngine.Object.DontDestroyOnLoad(prefab);
            }
            VFXMod.instance.VFX.prefabs[_name] = prefab;
            return prefab;
        }

        private GameObject MakePrefab(string name, Texture2D tex, bool destroyOnEnd)
        {
            if (tex == null)
            {
                throw new Exception("VFXBuilder - Tex was null!");
            }
            if (tex.width == 0 || tex.height == 0)
            {
                throw new Exception("VFX Builder - Tex has no area!");
            }
            ParticleSystem particleSystem = HopeUtils.CreateParticleSystem(name, textures: new Texture2D[] { tex }).WithDelays(new float[] { _duration });
            var col = particleSystem.colorOverLifetime;
            col.enabled = _hasColor;
            if (_hasColor)
            {
                col.color = _color;
            }
            var size = particleSystem.sizeOverLifetime;
            if (_hasSize)
            {
                size.enabled = true;
                if (_hasDifferentSizes)
                {
                    size.separateAxes = true;
                    size.x = _sizeX;
                    size.y = _sizeY;
                    size.z = _sizeZ;
                    size.xMultiplier = 1f;
                    size.yMultiplier = 1f;
                    size.zMultiplier = 1f;
                }
                else
                {
                    size.size = _sizeX;
                    size.sizeMultiplier = 1f;
                }
            }
            var vel = particleSystem.velocityOverLifetime;
            if (_hasVelocity)
            {
                vel.enabled = true;
                vel.x = _velX;
                vel.y = _velY;
                vel.z = _velZ;
                vel.xMultiplier = 1;
                vel.yMultiplier = 1;
                vel.zMultiplier = 1;
                vel.space = ParticleSystemSimulationSpace.World;
            }
            var rot = particleSystem.rotationOverLifetime;
            if (_hasRotation)
            {
                rot.enabled = true;
                rot.x = _rotX;
                rot.y = _rotY;
                rot.z = _rotZ;
                rot.xMultiplier = 1f;
                rot.yMultiplier = 1f;
                rot.zMultiplier = 1f;
            }
            var main = particleSystem.main;
            main.stopAction = destroyOnEnd ? ParticleSystemStopAction.Destroy : ParticleSystemStopAction.Disable;
            if (_hasGravity)
            {
                main.gravityModifier = _gravity;
                main.gravityModifierMultiplier = 1f;
            }
            if (_hasInitialRotation)
            {
                main.startRotationX = _initialRotX;
                main.startRotationY = _initialRotY;
                main.startRotationZ = _initialRotZ;
                main.startRotationXMultiplier = 1f;
                main.startRotationYMultiplier = 1f;
                main.startRotationZMultiplier = 1f;
            }
            return particleSystem.gameObject;
        }
    }
}