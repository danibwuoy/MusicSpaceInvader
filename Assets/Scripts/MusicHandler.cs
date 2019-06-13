using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicHandler : MonoBehaviour
{
    private AudioSource audioSource;
    private float[] samples;
    private float[] normalizedSamples;
    public Camera camera;
    public Gradient gradient;
    public Gradient starFieldGradient;
    public Gradient cameraGradient;
    public CameraShake cameraShake;
    private float MAX_SIZE = 50f;

    #region Starfields
    private float[] clouds;
    private float[] furtherStars;
    private float[] farStars;
    private float[] nearStars;
    #region Starfield Particles
    public ParticleSystem cloudsParticles;
    public ParticleSystem furtherStarParticles;
    public ParticleSystem farStarParticles;
    public ParticleSystem nearStarParticles;
    #endregion
    #region Starfiled StartSpeed
    private float cloudSpeed;
    private float furtherStarsSpeed;
    private float farStarsSpeed;
    private float nearStarsSpeed;
    #endregion
    #endregion

    #region SideElements
    private float[] sidePanels;
    private float[] cameraRegions;
    #endregion

    #region Test
    private Vector3 originPosition;
    private Quaternion originRotation;
    public float shake_decay;
    public float shake_intensity;
    private Vector3 initialPosition;
    private Quaternion initialRotation;
    #endregion

    // Use this for initialization
    void Start()
    {
        if (MusicPlayer.instance != null)
        {
            audioSource = MusicPlayer.instance.music;
        }

        samples = new float[1024];
        normalizedSamples = new float[1024];
        clouds = new float[172];
        furtherStars = new float[172];
        farStars = new float[172];
        furtherStars = new float[172];
        sidePanels = new float[168];
        cameraRegions = new float[168];

        cloudSpeed = cloudsParticles.startSpeed;
        furtherStarsSpeed = furtherStarParticles.startSpeed;
        farStarsSpeed = farStarParticles.startSpeed;
        nearStarsSpeed = nearStarParticles.startSpeed;

        initialPosition = camera.transform.position;
        initialRotation = camera.transform.rotation;
    }

    // Update is called once per frame
    void Update()
    {
        ProcessMusic();

        if (shake_intensity > 0)
        {
            camera.transform.position = originPosition + Random.insideUnitSphere * shake_intensity;
            camera.transform.rotation = new Quaternion(
            originRotation.x + Random.Range(-shake_intensity, shake_intensity) * .2f,
            originRotation.y + Random.Range(-shake_intensity, shake_intensity) * .2f,
            originRotation.z + Random.Range(-shake_intensity, shake_intensity) * .2f,
            originRotation.w + Random.Range(-shake_intensity, shake_intensity) * .2f);
            shake_intensity -= shake_decay;
        }

        //float totalFrequency = 0;
        ////MAX_SIZE = GetMaxAmplitude(samples);
        //for (int i = 0; i < samples.Length; i++)
        //{
        //    totalFrequency += Mathf.Clamp(samples[i] * (MAX_SIZE + i * i), 0, MAX_SIZE);
        //}

        //camera.backgroundColor = gradient.Evaluate(totalFrequency / samples.Length / MAX_SIZE);
        //Debug.Log(totalFrequency / samples.Length / MAX_SIZE);
    }

    private void ProcessMusic()
    {
        if (audioSource)
        {
            audioSource.GetSpectrumData(this.samples, 0, FFTWindow.BlackmanHarris);

            for (int i = 0; i < samples.Length; i++)
            {
                normalizedSamples[i] = Mathf.Clamp(samples[i] * (MAX_SIZE + i * i), 0, MAX_SIZE) / MAX_SIZE;
            }

            CreateMusicRegions();
            AssignColorsToParticles();
            AssignColorsToCamera();
        }
    }

    private void CreateMusicRegions()
    {
        clouds = GetSamples(0, 172, normalizedSamples);
        furtherStars = GetSamples(172, 172, normalizedSamples);
        farStars = GetSamples(344, 172, normalizedSamples);
        nearStars = GetSamples(516, 172, normalizedSamples);
        sidePanels = GetSamples(688, 168, normalizedSamples);
        cameraRegions = GetSamples(857, 168, normalizedSamples);
    }

    private float[] GetSamples(int startIndex, int length, float[] samples)
    {
        int counter = 0;
        float[] tempSamples = new float[length];

        for (int i = startIndex; i < startIndex + length - 1; ++i)
        {
            tempSamples[counter] = samples[i];
            ++counter;
        }

        return tempSamples;
    }

    private void AssignColorsToParticles()
    {
        float tempAverage = GetAverageSampleInRegion(clouds);
        cloudsParticles.startColor = starFieldGradient.Evaluate(tempAverage);
        cloudsParticles.startSpeed = cloudSpeed * (2f + tempAverage);

        tempAverage = GetAverageSampleInRegion(furtherStars);
        furtherStarParticles.startColor = starFieldGradient.Evaluate(tempAverage);
        furtherStarParticles.startSpeed = furtherStarsSpeed * (2f + tempAverage);

        tempAverage = GetAverageSampleInRegion(farStars);
        farStarParticles.startColor = starFieldGradient.Evaluate(tempAverage);
        farStarParticles.startSpeed = farStarsSpeed * (2f + tempAverage);

        tempAverage = GetAverageSampleInRegion(nearStars);
        nearStarParticles.startColor = starFieldGradient.Evaluate(tempAverage);
        nearStarParticles.startSpeed = nearStarsSpeed * (2f + tempAverage);

        Debug.Log("Clouds: " + GetAverageSampleInRegion(clouds));
        Debug.Log("Further Stars: " + GetAverageSampleInRegion(furtherStars));
        Debug.Log("Far Stars: " + GetAverageSampleInRegion(farStars));
        Debug.Log("Near Stars: " + GetAverageSampleInRegion(nearStars));
    }

    private void AssignColorsToCamera()
    {
        float tempAverage = GetAverageSampleInRegion(cameraRegions);
        camera.backgroundColor = cameraGradient.Evaluate(tempAverage);

        if (MusicPlayer.instance.musicType == 1)
        {
            if (tempAverage > 0.95f)
            {
                Shake(0.0125f, 0.001f);
                //EnemyResponder.instance.ShootLaser();
                //ShakeCamera(0.15f, 0.4f);
            }
        }

        else if (MusicPlayer.instance.musicType == 0)
        {
            if (tempAverage > 0.95f)
            {
                Shake(0.0100f, 0.0015f);
                //EnemyResponder.instance.ShootLaser();
                //ShakeCamera(0.15f, 0.4f);
            }
        }

        else
        {
            if (tempAverage > 0.95f)
            {
                Shake(0.0125f, 0.001f);
                //EnemyResponder.instance.ShootLaser();
                //ShakeCamera(0.15f, 0.4f);
            }
        }
    }

    private IEnumerator ShakeCamera(float duration, float magnitude)
    {
        Vector3 orignalPosition = camera.transform.position;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            camera.transform.position = new Vector3(x, y, -10f);
            elapsed += Time.deltaTime;
            yield return 0;
        }

        camera.transform.position = orignalPosition;

        //StartCoroutine(cameraShake.Shake(0.75f, 1.4f));
    }

    void Shake(float intensity, float decay)
    {
        originPosition = initialPosition;
        originRotation = initialRotation;
        shake_intensity = intensity;
        shake_decay = decay;
    }

    private float GetAverageSampleInRegion(float[] samples)
    {
        float tempMax = 0;

        for (int i = 0; i < samples.Length; ++i)
        {
            tempMax += samples[i];
        }

        return tempMax / samples.Length;
    }

    private float GetMaxAmplitude(float[] arraySamples)
    {
        float tempMax = arraySamples[0];

        for (int i = 0; i < arraySamples.Length; ++i)
        {
            if (tempMax < arraySamples[i])
            {
                tempMax = arraySamples[i];
            }
        }

        return tempMax;
    }

}
