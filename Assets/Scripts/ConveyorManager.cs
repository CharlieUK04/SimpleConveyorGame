using UnityEngine;

public class ConveyorManager : MonoBehaviour
{
    [SerializeField] private Material conveyorMaterialSet;

    [SerializeField] private float[] availableSpeeds = { 1f, 2f, 3f };
    public float getAvailableSpeed(int index) { return availableSpeeds[index]; }
    
    private ConveyorMateiral[] conveyorMateirals;
    public Material GetConveyorMaterial(int speed, bool isFlipped) 
    {return isFlipped ? conveyorMateirals[speed].flippedMaterial : conveyorMateirals[speed].baseMaterial;}

    private class ConveyorMateiral
    {
        public Material baseMaterial;
        public Material flippedMaterial;
        public float currentOffset = 0f;
    }
    
    
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        //Creates 2 material instances of each material speed, one flipped and one not 
        //This loads all needed materials at the start, and prevents duplicate materials.
        conveyorMateirals = new ConveyorMateiral[availableSpeeds.Length];
        for (int i = 0; i < conveyorMateirals.Length; i++)
        {
            conveyorMateirals[i] = new ConveyorMateiral();
            conveyorMateirals[i].baseMaterial = new Material(conveyorMaterialSet);
            conveyorMateirals[i].flippedMaterial = new Material(conveyorMaterialSet);
            conveyorMateirals[i].baseMaterial.SetTextureScale("_BaseMap", new Vector2(1f, 1f));
            conveyorMateirals[i].flippedMaterial.SetTextureScale("_BaseMap", new Vector2(-1f, 1f));
        }
        //---------------------------------------------------------
        
        
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < conveyorMateirals.Length; i++)
        {
            conveyorMateirals[i].baseMaterial.SetTextureOffset("_BaseMap", new Vector2(conveyorMateirals[i].currentOffset, 0f));
            conveyorMateirals[i].flippedMaterial.SetTextureOffset("_BaseMap", new Vector2(conveyorMateirals[i].currentOffset, 0f));
            
            conveyorMateirals[i].currentOffset += (availableSpeeds[i] * 1.5f) * Time.deltaTime;
            //Keeps the value between 0 and 1, since anything outside this range is visibly the same as anything between 0 and 1
            //Also prevents floating point errors and overflows from infinite incrementing
            conveyorMateirals[i].currentOffset = ((conveyorMateirals[i].currentOffset % 1f) + 1f) % 1f;
        }
    }
}
