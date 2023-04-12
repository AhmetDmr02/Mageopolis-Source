using System.Collections;
using UnityEngine;

public class EffectManager : MonoBehaviour
{
    public static EffectManager instance;
    [SerializeField] private ParticleSystem diceGroundHitEffect1;
    [SerializeField] private ParticleSystem diceGroundHitEffect2;
    [SerializeField] private ParticleSystem middleChargerFireEffect;
    [Header("Prefab Section")]
    [SerializeField] private GameObject landMarker;
    [SerializeField] private GameObject highlightEffect;
    [SerializeField] private GameObject moneyTracePrefab;
    [SerializeField] private GameObject resetLandEffect;
    [SerializeField] private GameObject wipeEffect;
    [SerializeField] private GameObject treeBlossom;
    [SerializeField] private GameObject protectionBlob;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(this.gameObject);
    }
    public void PlayDiceHitEffect()
    {
        diceGroundHitEffect1.Play();
        diceGroundHitEffect2.Play();
    }
    public void createLandMarkAt(int index)
    {
        GameObject go = Instantiate(landMarker);
        Vector3 stayPos = BoardMoveManager.instance.LandGameobject[index].GetComponent<PlotClass>().slidePos;
        go.transform.position = new Vector3(stayPos.x, 3.5f, stayPos.z);
    }
    public void CreateLandWipeEffect(Vector3 pos)
    {
        GameObject go = Instantiate(resetLandEffect);
        go.transform.position = pos;
        go.AddComponent<DestroyMe>().destroyMeAfterSeconds(5f);
    }
    public void createMoneyTraces(Vector3 whereToSpawn, Vector3 whereToLerp, int moneyTraceCount, Color32 traceColor)
    {
        StartCoroutine(moneyTracesBackend(whereToSpawn, whereToLerp, moneyTraceCount, traceColor));
    }
    IEnumerator moneyTracesBackend(Vector3 whereToSpawn, Vector3 whereToLerp, int moneyTraceCount, Color32 traceColor)
    {
        for (int i = 0; i < moneyTraceCount; i++)
        {
            //Math Stuff
            float angle = i * Mathf.PI * 2f / moneyTraceCount;
            Vector3 pos = new Vector3(Mathf.Sin(angle), 0f, Mathf.Cos(angle)) * 5;
            GameObject newObj = Instantiate(moneyTracePrefab, whereToSpawn + pos + Vector3.up * 5, Quaternion.identity);
            traceColor.a = 50;

            newObj.GetComponent<MeshRenderer>().material.color = traceColor;
            newObj.GetComponent<TrailRenderer>().material.color = traceColor;

            //Particle Effect Color
            ParticleSystem.MainModule settings = newObj.transform.GetChild(0).GetComponent<ParticleSystem>().main;
            settings.startColor = new ParticleSystem.MinMaxGradient(traceColor);
            for (int x = 0; x < newObj.transform.GetChild(0).childCount; x++)
            {
                ParticleSystem.MainModule TempSettings = newObj.transform.GetChild(0).GetChild(x).GetComponent<ParticleSystem>().main;
                TempSettings.startColor = new ParticleSystem.MinMaxGradient(traceColor);
            }

            newObj.GetComponent<AudioSource>().pitch = Random.Range(0.90f, 1.1f);
            newObj.GetComponent<AudioSource>().Play();

            newObj.GetComponent<MoneyTraceObject>().StartLerping(whereToLerp, 0.15f);
            float delay = Random.Range(0.3f, 0.7f);
            yield return new WaitForSeconds(delay);
        }
    }
    public GameObject createHighlightParticle(PlotClass pc)
    {
        GameObject go = Instantiate(highlightEffect);
        go.transform.position = pc.slidePos;
        go.transform.rotation = pc.gameObject.transform.rotation;
        return go;
    }
    public void createWipeEffect(Vector3 vec)
    {
        Transform t = Instantiate(wipeEffect).transform;
        t.position = vec;
        t.gameObject.AddComponent<DestroyMe>().destroyMeAfterSeconds(60);
    }
    public void createBlossomEffect(Vector3 vec)
    {
        Vector3 fixedVec = new Vector3(vec.x, vec.y - 0.94f, vec.z);
        Transform t = Instantiate(treeBlossom).transform;
        t.position = fixedVec;
        t.gameObject.AddComponent<DestroyMe>().destroyMeAfterSeconds(10);
    }
    public void createProtectionBlob(Vector3 vec, int plotIndex)
    {
        Vector3 fixedVec = new Vector3(vec.x, vec.y - 0.94f, vec.z);
        Transform t = Instantiate(protectionBlob).transform;
        t.position = fixedVec;
        BarrierVFX barrierVFX = t.gameObject.AddComponent<BarrierVFX>();
        barrierVFX.ownedPlotId = plotIndex;
        barrierVFX.subscribeEvent();
    }
    public void PlayMiddleChargerEffect()
    {
        middleChargerFireEffect.Play();
    }
}
