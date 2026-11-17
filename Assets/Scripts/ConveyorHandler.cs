using UnityEngine;

public class ConveyorHandler : MonoBehaviour
{
    [SerializeField] private Material conveyorMaterialSet;

    [SerializeField] private float[] availableSpeeds = { 1f, 2f, 3f };
    
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
            //conveyorMateirals[i] = new ConveyorMateiral();
            Debug.Log(conveyorMateirals[i]);
            conveyorMateirals[i].baseMaterial.SetTextureOffset("_BaseMap", new Vector2(conveyorMateirals[i].currentOffset, 0f));
            conveyorMateirals[i].flippedMaterial.SetTextureOffset("_BaseMap", new Vector2(conveyorMateirals[i].currentOffset, 0f));
            
            conveyorMateirals[i].currentOffset += availableSpeeds[i] / 100;
        }
    }
}
