using System.Collections;
using UnityEngine;

public class DemonicAltarController : MonoBehaviour
{

    public enum Showlightning { on, off };
    public Showlightning lightningEffects;
    [Space(10)]

    public Renderer altarRend;
    public ParticleSystem runeCircleParticles, runeCircleExplosionParticles, portalParticles, lightningParticles, sparksParticles;
    public Transform runeCircleTF;
    public Light portalLight, explosionLight;
    public AudioSource effectsAudio1, effectsAudio2;
    public AudioClip[] effectsClips;

    private Vector3 runeCircleStartPosition = new Vector3(0.18f, 1.2f, 0), runeCircleEndPosition = new Vector3(0.18f, 6.5f, 0);
    private bool activateAltar, activateCircle, portalActive, inTransition, checkLightIntensity, flickerLight, portalAudioFade;
    private float targetValueColor, runeCircleSpeed, randLightning, targetLightIntensity, portalAudioStartVolume, explosionLightTimer;
    private Material altarMat;
    private Color matColor = new Color(0, 0, 0, 0);

    private void Start()
    {
        altarMat = altarRend.material;
        portalAudioStartVolume = effectsAudio2.volume;
    }

    private void FixedUpdate()
    {
        if (inTransition)
        {
            //fades the Emission of the alter material in and out.
            if (matColor[3] != targetValueColor)
            {
                float ra = Mathf.MoveTowards(matColor[3], targetValueColor, 0.2f * Time.deltaTime);
                matColor[0] = ra;//R
                matColor[1] = ra * 0.3f;//G
                //B doesn`t change
                matColor[3] = ra;//A
                altarMat.SetColor("_EmissionColor", matColor);
            }
            else
            {
                if (targetValueColor == 0)//when deactivate
                    inTransition = false;
            }
        }

        if (activateCircle)
        {
            if (runeCircleTF.localPosition != runeCircleEndPosition)
            {
                //spinning circle moving up
                runeCircleParticles.Play();

                runeCircleSpeed += 0.4f * Time.deltaTime;

                runeCircleTF.localPosition = Vector3.MoveTowards(runeCircleTF.localPosition, runeCircleEndPosition, runeCircleSpeed * Time.deltaTime);
                runeCircleTF.Rotate(runeCircleTF.up, (runeCircleSpeed * 205) * Time.deltaTime);
            }
            else //spinning circle in top position.
            {
                if (!portalActive)
                {
                    portalParticles.Play();

                    runeCircleExplosionParticles.Emit(30);
                    Invoke("LightExplode", 0.6f);

                    runeCircleParticles.Clear();
                    runeCircleParticles.Stop();

                    portalActive = true;
                    activateCircle = false;
                    checkLightIntensity = true;

                    if (inTransition)//when activating
                        inTransition = false;
                }
            }
        }

        if (portalActive)
        {
            if (!effectsAudio2.isPlaying)//rumbling portal sound
            {
                effectsAudio2.clip = effectsClips[1];
                effectsAudio2.volume = portalAudioStartVolume;
                effectsAudio2.Play();
            }

            if (explosionLight.intensity > 0)
                explosionLight.intensity = Mathf.MoveTowards(explosionLight.intensity, 0, 8f * Time.deltaTime);

            //Lightning
            if (lightningEffects == Showlightning.on)
            {
                if (randLightning <= 0)
                {
                    portalLight.intensity = 1;
                    flickerLight = true;

                    effectsAudio1.PlayOneShot(effectsClips[2], 1f);

                    lightningParticles.Emit(1);
                    sparksParticles.Emit(30);

                    randLightning = Random.Range(4f, 12f);//random time till next lightning strike
                }
                else if (!checkLightIntensity)
                {
                    if (flickerLight)
                    {
                        if (portalLight.intensity != targetLightIntensity)
                            portalLight.intensity = Mathf.MoveTowards(portalLight.intensity, targetLightIntensity, 20f * Time.deltaTime);
                        else flickerLight = false;
                    }
                    randLightning -= Time.deltaTime;
                }
            }
        }
        else
        {
            //fade out portal audio
            if (portalAudioFade)
            {
                if (effectsAudio2.volume > 0)
                {
                    effectsAudio2.volume -= 0.12f * Time.deltaTime;
                }
                else portalAudioFade = false;
            }
        }

        if (checkLightIntensity)
        {
            if (portalLight.intensity != targetLightIntensity)
                portalLight.intensity = Mathf.MoveTowards(portalLight.intensity, targetLightIntensity, 1.6f * Time.deltaTime);
            else checkLightIntensity = false;
        }
    }

    //Call this to activate/Deactivate the altar(effects).
    public void ActivateDeactivateAltar()
    {
        if (inTransition)//wait for the effects to fully play out before being able activate/Deactivate again.
            return;

        activateAltar = !activateAltar;
        if (activateAltar)
        {
            effectsAudio2.Stop();
            effectsAudio1.PlayOneShot(effectsClips[0], 1f);
            activateCircle = true;

            targetValueColor = 1;
            targetLightIntensity = 4;
            randLightning = 6;//little delay for the portal to form befor the lightning starts.
        }
        else //Deactivate Altar
        {
            activateCircle = false;

            targetValueColor = 0;
            runeCircleSpeed = 0;
            targetLightIntensity = 0;

            portalActive = false;
            checkLightIntensity = true;

            portalParticles.Stop();

            runeCircleParticles.Clear();
            runeCircleParticles.Stop();
            runeCircleTF.localPosition = runeCircleStartPosition;

            portalAudioFade = true;
        }

        inTransition = true;
    }

    void LightExplode()
    {
        explosionLight.intensity = 6;
    }
}
