using System;
using System.Collections.Generic;
using System.Linq;
using Burk.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Sudoku
{
    public class ParticleManager : MonoBehaviourSingleton<ParticleManager>
    {
        public enum ParticleType
        {
            Correct,
            Incorrect,
            What
        }
        
        [Serializable]
        public class UIImageParticle
        {
            public Sprite sprite;
            public ParticleType type;
            public float startRotation;
            public float rotateSpeed;
            public float lifetime;
            public bool gravity;
            public AnimationCurve sizeCurve;
            public AnimationCurve alphaCurve;
            public AnimationCurve velocityCurve;
            public AnimationCurve rotationCurve;
        }

        public class LiveParticle
        {
            public UIImageParticle particleData;
            public GameObject gameObject;
            public Image image;
            public RectTransform rectTransform;
            public float lifetime;
            public bool gravity;

            public Vector2 velocity;            
            public float rotateSpeed;

            public void Start()
            {
                I.liveParticles.Add(this);
            }
            
            public void Update()
            {
                float gm = gravity ? (particleData.lifetime - lifetime) * 1600 : 0;
                lifetime -= Time.deltaTime;
                float t = 1-lifetime/particleData.lifetime;
                rectTransform.Rotate(0, 0, rotateSpeed *(particleData.rotationCurve.Evaluate(t) * Time.deltaTime));
                rectTransform.position +=
                    (Vector3)(velocity * (particleData.velocityCurve.Evaluate(t) * Time.deltaTime)) +
                    gm * Time.deltaTime * Vector3.down;
                rectTransform.localScale = Vector3.one * (particleData.sizeCurve.Evaluate(t));
                image.color = new Color(image.color.r, image.color.g, image.color.b, particleData.alphaCurve.Evaluate(t));
                if (lifetime <= 0)
                {
                    Remove();
                    Destroy(gameObject);
                }
            }

            private void Remove()
            {
                I.removeQueue.Enqueue(this);
            }
        }

        public RectTransform parentTransform;
        
        public Queue<LiveParticle> removeQueue;
        public List<LiveParticle> liveParticles;
        public List<UIImageParticle> particles;
        public Dictionary<ParticleType, UIImageParticle> particleDictionary;

        private void Start()
        {
            removeQueue = new Queue<LiveParticle>();
            liveParticles = new List<LiveParticle>();
            particleDictionary = new Dictionary<ParticleType, UIImageParticle>();
            foreach (var particle in particles)
            {
                particleDictionary.Add(particle.type, particle);
            }
        }

        public void Update()
        {
            if(removeQueue.Count > 0)
            {
                while (removeQueue.Count > 0)
                {
                    var particle = removeQueue.Dequeue();
                    liveParticles.Remove(particle);
                }
            }
            
            if(liveParticles.Count > 0)
            {
                foreach (var particle in liveParticles)
                {
                    particle.Update();
                }
            }
        }

        public static void SpawnParticle(ParticleType type, Vector2 position, Vector2 startOffset, float size, Vector2 startVelocity)
        {
            LiveParticle particle = new LiveParticle();
            particle.gameObject = new GameObject(type + " particle");
            particle.image = particle.gameObject.AddComponent<Image>();
            particle.rectTransform = particle.gameObject.GetComponent<RectTransform>();
            particle.rectTransform.SetParent(I.parentTransform);
            
            var particleData = I.particleDictionary[type];
            
            particle.particleData = particleData;
            particle.gravity = particleData.gravity;
            particle.lifetime = particleData.lifetime;
            particle.rectTransform.position = position + startOffset;
            particle.rectTransform.sizeDelta = new Vector2(size, size);
            particle.rectTransform.rotation = Quaternion.Euler(0, 0, particleData.startRotation);
            particle.image.sprite = particleData.sprite;
            particle.velocity = startVelocity;
            particle.Start();
        }
        
    }
}