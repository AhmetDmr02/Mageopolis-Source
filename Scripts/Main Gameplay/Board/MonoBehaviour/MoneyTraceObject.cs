using UnityEngine;

public class MoneyTraceObject : MonoBehaviour
{
    private float speed;
    private bool isLerping;
    [SerializeField] private AudioSource mystTravel, mystCollected;
    [SerializeField] private float wiggleFrequency = 1f;
    [SerializeField] float wiggleMagnitude = 25f;
    private Vector3 startPosition, targetPosition;
    private Vector3 previousPos;
    private Vector3 wiggleOffset;

    private float startTime;
    private float journeyFraction;

    private bool mystCollectedPlayed;
    private bool mystMidwayPlayed;
    void Start()
    {
        // Seed random number generator to ensure different wiggle patterns every time
        Random.InitState(System.DateTime.Now.Millisecond);
    }
    void Update()
    {
        if (!isLerping) return;
        journeyFraction += speed * Time.deltaTime;

        // Ensure fraction stays between 0 and 1
        journeyFraction = Mathf.Clamp01(journeyFraction);

        // Calculate the current position of the cube based on the journey fraction
        Vector3 currentPos = Vector3.Lerp(startPosition, targetPosition, journeyFraction);

        // Calculate the wiggle offset using Perlin noise
        Vector3 wiggleOffset = new Vector3(
            Mathf.PerlinNoise(journeyFraction * wiggleFrequency, 0) * 2 - 1,
            Mathf.PerlinNoise(journeyFraction * wiggleFrequency, 1) * 2 - 1,
            Mathf.PerlinNoise(journeyFraction * wiggleFrequency, 2) * 2 - 1
        );

        // Scale the wiggle offset by the magnitude
        wiggleOffset *= wiggleMagnitude;

        // Add the wiggle offset to the current position
        currentPos += wiggleOffset;

        this.transform.position = currentPos;

        if (journeyFraction >= 0.4f && journeyFraction < 0.8f)
        {
            // Lerp is done
            if (mystMidwayPlayed) return;
            mystMidwayPlayed = true;
            mystTravel.Play();
        }
        else
        if (journeyFraction >= 0.95f && journeyFraction < 1)
        {
            // Lerp is done
            if (mystCollectedPlayed) return;
            mystCollectedPlayed = true;
            mystCollected.Play();
        }
        else
        if (journeyFraction >= 1.0f)
        {
            // Lerp is done
            isLerping = false;
            Destroy(this.gameObject, 0.2f);
        }
    }
    public void StartLerping(Vector3 targetPosition, float speed)
    {
        this.targetPosition = new Vector3(targetPosition.x, targetPosition.y, targetPosition.z);
        this.speed = Random.Range(speed, speed + 0.1f);
        startPosition = transform.position;
        journeyFraction = 0.0f;
        startTime = Time.time;
        isLerping = true;
    }
}
