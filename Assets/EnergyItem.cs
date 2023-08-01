using UnityEngine;

public class EnergyItem : MonoBehaviour
{
    public float energyAmount = 10f; // Default energy amount
    public float decay = 0f;
    public bool canDecay = false;

    private void Start()
    {
        decay = 0f;
    }

    private void Update()
    {
        decay += Time.deltaTime;

        // Rotate the energy item (you can add more effects as needed)
        // transform.Rotate(Vector3.up, 60f * Time.deltaTime);
        if(decay >= 60f && canDecay){
            Destroy(gameObject);
        }

        if(energyAmount <= 0){
            energyAmount = 10;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Check if the collider that entered the trigger is a cell
        Cell cell = collision.gameObject.GetComponent<Cell>();
        if (cell != null && cell.canColectEnergy)
        {
            // Collect energy if it's a cell
            cell.energy += energyAmount;
            // Destroy the energy item
            Destroy(gameObject);
        }
    }

    // Called when another cell collects the energy item
    public void CollectEnergy(Cell collectorCell)
    {
        // Add the energy amount to the collector cell's energy
        collectorCell.energy += energyAmount;

        // Destroy the energy item once collected
        Destroy(gameObject);
    }
}
