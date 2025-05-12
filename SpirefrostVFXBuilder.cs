using System;
using System.IO;
using System.Linq;
using Deadpan.Enums.Engine.Components.Modding;
using FMOD;
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

        internal SpirefrostVFXBuilder WithColorGradient(params Color[] colors)
        {
            if (colors.Length == 0)
            {
                return this;
            }
            if (colors.All(color => color == colors[0]))
            {
                return WithColor(colors[0]);
            }
            Gradient grad = new Gradient
            {
                colorKeys = colors.Select((color, i) => new GradientColorKey(color, ((float)i) / (colors.Length-1))).ToArray(),
                alphaKeys = colors.Select((color, i) => new GradientAlphaKey(color.a, ((float)i) / (colors.Length-1))).ToArray(),
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

        internal SpirefrostVFXBuilder WithVelocityGradient(params Vector3[] vectors)
        {
            return WithVelocityGradient(false, vectors);
        }

        internal SpirefrostVFXBuilder WithVelocityGradient(bool smooth, params Vector3[] vectors)
        {
            if (vectors.Length == 0)
            {
                return this;
            }
            if (vectors.All(vec => vec == vectors[0]))
            {
                return WithVelocity(vectors[0]);
            }
            _velX = new ParticleSystem.MinMaxCurve(1, BuildCurve(smooth, vectors.Select(vec => vec.x).ToArray()));
            _velY = new ParticleSystem.MinMaxCurve(1, BuildCurve(smooth, vectors.Select(vec => vec.y).ToArray()));
            _velZ = new ParticleSystem.MinMaxCurve(1, BuildCurve(smooth, vectors.Select(vec => vec.z).ToArray()));
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

        internal SpirefrostVFXBuilder WithRandomVelocityGradient(Vector3[] from, Vector3[] to)
        {
            return WithRandomVelocityGradient(false, from, to);
        }

        internal SpirefrostVFXBuilder WithRandomVelocityGradient(bool smooth, Vector3[] from, Vector3[] to)
        {
            _velX = new ParticleSystem.MinMaxCurve(1, BuildCurve(smooth, from.Select(vec => vec.x).ToArray()), BuildCurve(smooth, to.Select(vec => vec.x).ToArray()));
            _velY = new ParticleSystem.MinMaxCurve(1, BuildCurve(smooth, from.Select(vec => vec.y).ToArray()), BuildCurve(smooth, to.Select(vec => vec.y).ToArray()));
            _velZ = new ParticleSystem.MinMaxCurve(1, BuildCurve(smooth, from.Select(vec => vec.z).ToArray()), BuildCurve(smooth, to.Select(vec => vec.z).ToArray()));
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

        internal SpirefrostVFXBuilder WithSizeGradient(params float[] sizes)
        {
            return WithSizeGradient(false, sizes);
        }

        internal SpirefrostVFXBuilder WithSizeGradient(bool smooth, params float[] sizes)
        {
            if (sizes.Length == 0)
            {
                return this;
            }
            if (sizes.All(size => size == sizes[0]))
            {
                return WithSize(sizes[0]);
            }
            _sizeX = new ParticleSystem.MinMaxCurve(1, BuildCurve(smooth, sizes));
            _hasSize = true;
            return this;
        }

        internal SpirefrostVFXBuilder WithSizeGradient(params Vector3[] vectors)
        {
            return WithSizeGradient(false, vectors);
        }

        internal SpirefrostVFXBuilder WithSizeGradient(bool smooth, params Vector3[] vectors)
        {
            if (vectors.Length == 0)
            {
                return this;
            }
            if (vectors.All(vec => vec.x == vec.y && vec.y == vec.z))
            {
                return WithSizeGradient(smooth, vectors.Select(vec => vec.x).ToArray());
            }
            _sizeX = new ParticleSystem.MinMaxCurve(1, BuildCurve(smooth, vectors.Select(vec => vec.x).ToArray()));
            _sizeY = new ParticleSystem.MinMaxCurve(1, BuildCurve(smooth, vectors.Select(vec => vec.y).ToArray()));
            _sizeZ = new ParticleSystem.MinMaxCurve(1, BuildCurve(smooth, vectors.Select(vec => vec.z).ToArray()));
            _hasSize = true;
            _hasDifferentSizes = true;
            return this;
        }

        internal SpirefrostVFXBuilder WithRandomSize(float from, float to)
        {
            _sizeX = new ParticleSystem.MinMaxCurve(from, to);
            _hasSize = true;
            return this;
        }

        internal SpirefrostVFXBuilder WithRandomSize(Vector3 from, Vector3 to)
        {
            if (from.x == from.y && from.y == from.x && to.x == to.y && to.y == to.z)
            {
                return WithRandomSize(from.x, to.x);
            }
            _sizeX = new ParticleSystem.MinMaxCurve(from.x, to.x);
            _sizeY = new ParticleSystem.MinMaxCurve(from.y, to.y);
            _sizeZ = new ParticleSystem.MinMaxCurve(from.z, to.z);
            _hasSize = true;
            _hasDifferentSizes = true;
            return this;
        }

        internal SpirefrostVFXBuilder WithRandomSizeGradient(float[] from, float[] to)
        {
            return WithRandomSizeGradient(false, from, to);
        }

        internal SpirefrostVFXBuilder WithRandomSizeGradient(bool smooth, float[] from, float[] to)
        {
            _sizeX = new ParticleSystem.MinMaxCurve(1, BuildCurve(smooth, from), BuildCurve(smooth, to));
            _hasSize = true;
            return this;
        }

        internal SpirefrostVFXBuilder WithRandomSizeGradient(Vector3[] from, Vector3[] to)
        {
            return WithRandomSizeGradient(false, from, to);
        }

        internal SpirefrostVFXBuilder WithRandomSizeGradient(bool smooth, Vector3[] from, Vector3[] to)
        {
            if (from.All(vec => vec.x == vec.y && vec.y == vec.z) && to.All(vec => vec.x == vec.y && vec.y == vec.z))
            {
                return WithRandomSizeGradient(smooth, from.Select(vec => vec.x).ToArray(), to.Select(vec => vec.x).ToArray());
            }
            _sizeX = new ParticleSystem.MinMaxCurve(1, BuildCurve(smooth, from.Select(vec => vec.x).ToArray()), BuildCurve(smooth, to.Select(vec => vec.x).ToArray()));
            _sizeY = new ParticleSystem.MinMaxCurve(1, BuildCurve(smooth, from.Select(vec => vec.y).ToArray()), BuildCurve(smooth, to.Select(vec => vec.y).ToArray()));
            _sizeZ = new ParticleSystem.MinMaxCurve(1, BuildCurve(smooth, from.Select(vec => vec.z).ToArray()), BuildCurve(smooth, to.Select(vec => vec.z).ToArray()));
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

        internal SpirefrostVFXBuilder WithRotationGradient(params Vector3[] vectors)
        {
            return WithRotationGradient(false, vectors);
        }

        internal SpirefrostVFXBuilder WithRotationGradient(bool smooth, params Vector3[] vectors)
        {
            if (vectors.Length == 0)
            {
                return this;
            }
            if (vectors.All(vec => vec == vectors[0]))
            {
                return WithRotation(vectors[0]);
            }
            _rotX = new ParticleSystem.MinMaxCurve(1, BuildCurve(smooth, vectors.Select(vec => vec.x).ToArray()));
            _rotY = new ParticleSystem.MinMaxCurve(1, BuildCurve(smooth, vectors.Select(vec => vec.y).ToArray()));
            _rotZ = new ParticleSystem.MinMaxCurve(1, BuildCurve(smooth, vectors.Select(vec => vec.z).ToArray()));
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

        internal SpirefrostVFXBuilder WithRandomRotationGradient(Vector3[] from, Vector3[] to)
        {
            return WithRandomRotationGradient(false, from, to);
        }

        internal SpirefrostVFXBuilder WithRandomRotationGradient(bool smooth, Vector3[] from, Vector3[] to)
        {
            _rotX = new ParticleSystem.MinMaxCurve(1, BuildCurve(smooth, from.Select(vec => vec.x).ToArray()), BuildCurve(smooth, to.Select(vec => vec.x).ToArray()));
            _rotY = new ParticleSystem.MinMaxCurve(1, BuildCurve(smooth, from.Select(vec => vec.y).ToArray()), BuildCurve(smooth, to.Select(vec => vec.y).ToArray()));
            _rotZ = new ParticleSystem.MinMaxCurve(1, BuildCurve(smooth, from.Select(vec => vec.z).ToArray()), BuildCurve(smooth, to.Select(vec => vec.z).ToArray()));
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

        internal SpirefrostVFXBuilder WithGravityGradient(params float[] values)
        {
            return WithGravityGradient(false, values);
        }

        internal SpirefrostVFXBuilder WithGravityGradient(bool smooth, params float[] values)
        {
            if (values.Length == 0)
            {
                return this;
            }
            if (values.All(val => val == values[0]))
            {
                return WithGravity(values[0]);
            }
            _gravity = new ParticleSystem.MinMaxCurve(1, BuildCurve(smooth, values));
            _hasGravity = true;
            return this;
        }

        internal SpirefrostVFXBuilder WithRandomGravity(float from, float to)
        {
            _gravity = new ParticleSystem.MinMaxCurve(from, to);
            _hasGravity = true;
            return this;
        }

        internal SpirefrostVFXBuilder WithRandomGravityGradient(float[] from, float[] to)
        {
            return WithRandomGravityGradient(false, from, to);
        }

        internal SpirefrostVFXBuilder WithRandomGravityGradient(bool smooth, float[] from, float[] to)
        {
            _gravity = new ParticleSystem.MinMaxCurve(1, BuildCurve(smooth, from), BuildCurve(smooth, to));
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

        private AnimationCurve BuildCurve(bool smooth, params float[] values)
        {
            if (values.Length == 0)
            {
                return AnimationCurve.Constant(0, 1, 0);
            }
            if (values.Length == 1)
            {
                return AnimationCurve.Constant(0, 1, values[0]);
            }
            float outVal = smooth ? 0 : (values[1] - values[0]) * (values.Length - 1);
            float inVal = smooth ? 0 : (values[values.Length - 1] - values[values.Length - 2]) * (values.Length - 1);
            return new AnimationCurve(values.Select((val, i) => new Keyframe(((float)i)/(values.Length-1), val, (i == values.Length-1) ? inVal : 0, (i == 0) ? outVal : 0)).ToArray());
        }
    }
}