using UnityEngine;

public class Cell : MonoBehaviour
{
    // Properties...
    public float maxHealth = 100f;
    public float maxEnergy = 100f;
    public float reproductionThreshold = 75f;
    public float reproductionEnergyCost = 60f;
    public float reproductionCooldown = 30f;
    public float suicideCountdown = 250f;
    public float heatGenerationRate = 5f;
    public float heatRadius = 5f;
    public float genoration = 0f;

    public float health;
    public float energy;
    public float timeSinceLastReproduction;

    // New realistic properties...
    public float metabolicRate = 3f;
    public float photosynthesisRate = 5f;
    public float photosynthesisEfficiency = 0.3f;
    public float respirationRate = 1f;
    public float heatResistance = 1f;
    public float coldResistance = 1f;
    public float heatProductionRate = 2f;
    public float heatProductionEfficiency = 0.1f;
    public float heatDissipationRate = 1f;
    public float baseSpeed = 2f;
    public float maxSpeed = 5f;
    public float speedDecayRate = 0.5f;
    public float speedAccelerationRate = 1f;
    public float defenseRating = 1f;
    public float attackRating = 1f;
    public float visionRadius = 10f;
    public float visionAngle = 180f;
    public float smellRadius = 5f;
    public float smellThreshold = 0.5f;
    public float hearingRadius = 5f;
    public float hearingThreshold = 0.5f;
    public float communicationRange = 20f;
    public float communicationEfficiency = 0.5f;
    public float remainingEnergy { get; private set; }
    public float remainingHealth { get; private set; }
    public float agingRate = 0.1f;
    public float lifespan = 300f;
    public float reproductionRange = 5f;

    private SpriteRenderer spriteRenderer;
    public Gradient speedColorGradient;
    public Color photosynthesisColor;

    public bool isPerformingPhotosynthesis = true;
    public bool isGeneratingHeat = true;
    public bool canHarnessHeat = false;
    public bool canColectEnergy = false;

    public float suicideTimer;
    public bool canSuicide = true;

    public Vector3 originalScale;

    public GameObject energyItemPrefab;
    public Environment environment;

    private void Start()
    {
        // Initialize the health and energy of the cell
        health = maxHealth;
        energy = maxEnergy/4;
        timeSinceLastReproduction = reproductionCooldown;
        suicideTimer = suicideCountdown;

        // Get the SpriteRenderer component to change the cell's color
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Initialize remaining energy and health for lifespan tracking
        remainingEnergy = maxEnergy;
        remainingHealth = maxHealth;

        reproductionCooldown = 15f;
        reproductionCooldown += Random.Range(-reproductionRange, reproductionRange);
        CellManager.instance.RegisterCell(this);
    }

    private void Update()
    {
        if(health < maxHealth){
            health += energy/250;
            energy -= energy/250;
        }

        if(canHarnessHeat && baseSpeed < 2){
            baseSpeed = Random.Range(2f, 2.5f);
        }

        if(canHarnessHeat){
            isGeneratingHeat = false;
        }

        if(baseSpeed < 0){
            baseSpeed = 0;
        }

        // Check if the cell's energy is below zero
        if (energy < 0f)
        {
            energy = 0f;
        }

        // Calculate photosynthesis rate based on sunlight and photosynthesis efficiency
        float sunlightIntensity = GetSunlightIntensity();
        float photosynthesisAmount = sunlightIntensity * photosynthesisRate * photosynthesisEfficiency;

        // Perform photosynthesis and respiration based on the cell's state
        if (isPerformingPhotosynthesis)
        {
            energy += photosynthesisAmount * Time.deltaTime;
        }
        else
        {
            energy -= respirationRate * Time.deltaTime;
        }

        // Check if the cell's energy is below zero
        if (energy < 0f)
        {
            // If energy is depleted, calculate the energy deficit
            float energyDeficit = Mathf.Abs(energy);
            // Take damage to health proportional to the energy deficit
            float damage = energyDeficit * 2f;
            TakeDamage(damage);
            energy = 0f;
        }

        // Increment the timer since the last reproduction
        timeSinceLastReproduction += Time.deltaTime;

        

        // Check if the cell has enough energy to reproduce and if the reproduction cooldown has passed
        if (energy >= reproductionThreshold && timeSinceLastReproduction >= reproductionCooldown)
        {
            // Create a new cell as offspring
            CreateOffspring();
            // Reset the timer after reproduction
            timeSinceLastReproduction = 0f;
            reproductionCooldown = 15f;
            reproductionCooldown += Random.Range(-reproductionRange, reproductionRange);
        }

        // Randomly move the cell if not performing photosynthesis
        if (!isPerformingPhotosynthesis)
        {
            MoveRandomly();
        }

        // Speed decay over time when not photosynthesizing
        // float speedDecay = isPerformingPhotosynthesis ? 0f : speedDecayRate * Time.deltaTime;
        // baseSpeed = Mathf.Max(baseSpeed - speedDecay, 0f);

        // Accelerate the cell's speed when photosynthesizing
        // float speedAcceleration = isPerformingPhotosynthesis ? speedAccelerationRate * Time.deltaTime : 0f;
        // baseSpeed = Mathf.Min(baseSpeed + speedAcceleration, maxSpeed);

        // Update the cell's color based on its speed
        UpdateCellColor();

        // Countdown to suicide
        suicideTimer -= Time.deltaTime;
        if (suicideTimer <= 0f && canSuicide)
        {
            // Time to die, perform suicide
            Die();
        }

        // Generate and dissipate heat if allowed
        if (isGeneratingHeat)
        {
            GenerateAndDissipateHeat();
        }

        // Aging of the cell over time
        float agingAmount = agingRate * Time.deltaTime;
        remainingEnergy -= agingAmount;
        remainingHealth -= agingAmount;

        // Check for natural death based on lifespan
        if (remainingEnergy <= 0f || remainingHealth <= 0f)
        {
            Die();
        }

        if(isPerformingPhotosynthesis && canHarnessHeat)
        {
            isPerformingPhotosynthesis = false;
        }

        if(energy > maxEnergy){
            energy = maxEnergy;
        }

        transform.position = new Vector3(transform.position.x, transform.position.y, 0f);
    }

    private float GetSunlightIntensity()
    {
        // Implement your own logic to determine the sunlight intensity at the cell's position
        // For example, you can use Raycasts to detect the intensity based on light sources
        return 1f;
    }

    public void TakeDamage(float damage)
    {
        // Reduce the cell's health based on the damage amount
        health -= damage;

        // Check if the cell's health is below zero
        if (health <= 0f)
        {
            // Cell is dead, perform death actions (e.g., destroy the GameObject)
            Die();
        }
    }

    public void Die()
    {
        // Instantiate a collectible energy item
        GameObject energyItem = Instantiate(energyItemPrefab, transform.position, Quaternion.identity);

        // Set the energy amount of the energy item to be the remaining energy of the dying cell
        EnergyItem energyItemComponent = energyItem.GetComponent<EnergyItem>();
        if (energyItemComponent != null)
        {
            energyItemComponent.energyAmount = energy;
            energyItemComponent.canDecay = true;
            energyItem.transform.position = new Vector3(energyItem.transform.position.x, energyItem.transform.position.y, 4.5f);
        }

        environment.cells--;

        // Perform any other death actions here
        Destroy(gameObject);
    }

    public float offspringFail = 0f;

    private void CreateOffspring()
    {
        if(environment.cells <= 900f){

            // Deduct energy for the cost of reproduction
            energy -= reproductionEnergyCost;

            // Randomly select a spawn position
            Vector3 spawnPosition = transform.position + Random.insideUnitSphere * 10f;

            // Check if there are any colliders within a certain radius around the spawn position
            Collider2D[] colliders = Physics2D.OverlapCircleAll(spawnPosition, 1f); // Adjust the radius to your desired value

            // Check if the spawn position is already occupied by another cell
            bool isOccupied = false;
            foreach (Collider2D collider in colliders)
            {
                if (collider.gameObject != gameObject)
                {
                    // If there is a collider other than itself, the position is considered occupied
                    isOccupied = true;
                    break;
                }
            }

            if (!isOccupied)
            {
                environment.cells++;
                offspringFail = 0f;

                // Deduct energy for the cost of reproduction
                energy -= reproductionEnergyCost;

                // Create a new cell as offspring (a clone of the current cell)
                GameObject offspring = Instantiate(gameObject, transform.position + Random.insideUnitSphere * 2f, Quaternion.identity);
                Cell offspringCell = offspring.GetComponent<Cell>();

                // Modify the offspring's traits (genes) based on mutation or inheritance here
                // For simplicity, let's add random variation to speed and energy consumption
                offspringCell.health = health;
                offspringCell.energy = energy;

                // Random variation in speed and energy consumption for the offspring
                float energyVariationOffset = Random.Range(-5f, 5f);
                float healthVariationOffset = Random.Range(-5f, 5f);
                float speedVariationOffset = Random.Range(-0.2f, 0.2f);
                float energyConsumptionVariationOffset = Random.Range(-1f, 1f); // Updated to a valid range
                float reproductionCooldownVariationOffset = Random.Range(-0.1f, 0.1f);

                offspringCell.baseSpeed += speedVariationOffset;

                // Mutation chance for photosynthesis (1% chance)
                if (Random.Range(0f, 100f) <= 0.5f)
                {
                    if (offspringCell.isPerformingPhotosynthesis)
                    {
                        // If the offspring has photosynthesis, remove the capability
                        offspringCell.StopPhotosynthesis();
                    }
                    else
                    {
                        // If the offspring doesn't have photosynthesis, add the capability
                        offspringCell.StartPhotosynthesis();
                    }
                }

                offspringCell.maxHealth += healthVariationOffset;
                offspringCell.maxEnergy += energyVariationOffset * offspringCell.maxHealth / 100f;

                // Set the reproduction timer of the offspring to be controlled by the parent's reproduction timer
                offspringCell.timeSinceLastReproduction = timeSinceLastReproduction;
                // Set the suicide timer of the offspring to be 3 times the reproduction timer of the parent
                offspringCell.suicideTimer = reproductionCooldownVariationOffset * 3f;
                offspringCell.reproductionCooldown = 10f;

                // Set the heat generation and heat radius of the offspring to be the same as the parent
                offspringCell.isGeneratingHeat = isGeneratingHeat;
                //offspringCell.heatGenerationRate = heatGenerationRate;
                offspringCell.heatRadius = heatRadius;
                offspringCell.genoration = genoration + 1f;

                // Mutation chance for heat harnessing (1% chance)
                if (Random.Range(0f, 200f) <= 1f)
                {
                    if (offspringCell.canHarnessHeat)
                    {
                        // If the offspring can harness heat, remove the capability
                        offspringCell.canHarnessHeat = false;
                    }
                    else
                    {
                        // If the offspring doesn't have heat harnessing, add the capability
                        offspringCell.canHarnessHeat = true;
                    }
                }

                if (Random.Range(0f, 300f) <= 1.5f)
                {
                    if (offspringCell.canColectEnergy)
                    {
                        // If the offspring can harness heat, remove the capability
                        offspringCell.canColectEnergy = false;
                    }
                    else
                    {
                        // If the offspring doesn't have heat harnessing, add the capability
                        offspringCell.canColectEnergy = true;
                    }
                }
            }
            else
            {
                if(offspringFail < 6f){
                    offspringFail++;
                    CreateOffspring();
                }else{
                    offspringFail = 0f;
                }
            }
        }
    }

    private void OnDestroy()
    {
        // Unregister the cell from the CellManager when it's destroyed
        CellManager.instance.UnregisterCell(this);
    }

    private void MoveRandomly()
    {
        // Random movement of the cell
        Vector3 randomDirection = Random.insideUnitSphere;
        float randomSpeed = baseSpeed/environment.speed + Random.Range(-1f/environment.speed, 1f/environment.speed); // Updated to a valid range

        // Ensure the speed is not negative
        randomSpeed = Mathf.Max(randomSpeed, 0f);

        Vector3 movement = randomDirection * randomSpeed * Time.deltaTime;
        transform.Translate(movement);

        // Adjust energy consumption based on speed
        float energyConsumed = randomSpeed * metabolicRate;
        energy -= energyConsumed * Time.deltaTime;
    }

    private void UpdateCellColor()
    {
        float healthPercentage = health / maxHealth;
        float transparency = 1f - healthPercentage; // 1f means fully opaque, 0f means fully transparent

        // Calculate the scale factor based on the current max health and the original scale
        float scaleFactor = maxHealth / 100f;

        // Set the new scale of the cell based on the scale factor
        transform.localScale = Vector3.one * scaleFactor;

        // Check if the cell is performing photosynthesis and set the color accordingly
        if (isPerformingPhotosynthesis)
        {
            spriteRenderer.color = photosynthesisColor;
        }
        else
        {
            // Map the cell's speed to a color using the speedColorGradient
            Color cellColor = speedColorGradient.Evaluate(baseSpeed / 25f); // Adjusted the divisor to a valid value

            // Set the cell's color using the SpriteRenderer component
            spriteRenderer.color = cellColor;
        }
    }


    // Method to start photosynthesis
    public void StartPhotosynthesis()
    {
        // Immobilize the cell during photosynthesis
        isPerformingPhotosynthesis = true;
        baseSpeed = 0f;
    }

    // Method to stop photosynthesis
    public void StopPhotosynthesis()
    {
        // Enable cell movement after photosynthesis
        isPerformingPhotosynthesis = false;
        baseSpeed = 2f; // Set the base speed to the regular value or a desired value after photosynthesis
    }

    // Method to generate and dissipate heat
    private void GenerateAndDissipateHeat()
    {
        // Calculate the heat generation based on the cell's state
        float heatGeneration = heatProductionRate * (energy / maxEnergy) * heatProductionEfficiency;

        // Generate heat
        energy -= heatGeneration * Time.deltaTime;
        // Heat dissipation
        energy += heatDissipationRate * Time.deltaTime;

        // Cells within the heat radius can harness the heat generated
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, heatRadius);

        foreach (Collider2D collider in colliders)
        {
            if (collider.gameObject != gameObject)
            {
                Cell otherCell = collider.gameObject.GetComponent<Cell>();
                if (otherCell != null)
                {
                    // Calculate the heat transfer between cells
                    float heatTransferAmount = heatGenerationRate * heatResistance;
                    
                    // Cells capable of harnessing heat can receive and utilize it
                    if (otherCell.canHarnessHeat)
                    {
                        // Transfer heat to otherCell
                        otherCell.energy += heatTransferAmount/205f; 
                    }
                    else
                    {
                        // Otherwise, transfer the heat to the environment
                        // energy -= heatTransferAmount;
                    }
                }
            }
        }
    }
}
