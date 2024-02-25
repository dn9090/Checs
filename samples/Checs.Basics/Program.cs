using System;
using Checs;

// Create the manager which contains all entities, archetypes,
// queries and a specialized allocator.
// Dispose the entity manager to free the allocated memory.
using var manager = new EntityManager();

// Create an entity without any components.
var entity = manager.CreateEntity();

// Add a position, rotation and velocity component to the entity.
manager.AddComponentData(entity, new Position { x = 1f, y = 2f, z = 3f});
manager.AddComponentData(entity, new Rotation { x = 1f, y = 2f, z = 3f, w = 4f});
manager.AddComponentData<Velocity>(entity);

// Create an archetype which automatically adds the components to entities
// created with the archetype.
// It is faster than manually adding all components and operations
// on archetypes are batchable.
var archetype = manager.CreateArchetype(new[] {
	ComponentType.Of<Position>(),
	ComponentType.Of<Rotation>(),
	ComponentType.Of<Velocity>(),
	ComponentType.Of<ChildOf>()
});

// Create an entity with the archetype.
var child = manager.CreateEntity(archetype);

// Set the first entity as the parent value.
manager.SetComponentData(child, new ChildOf { parent = entity });

// Set the position value to that of the parent entity.
var position = manager.GetComponentData<Position>(entity);
manager.SetComponentData(child, position);

// Create multiple clones of the child which all have the same components with
// the same values as the cloned entity.
var children = new Entity[20];
manager.Instantiate(child, children);

// Set the position of all cloned children.
manager.SetComponentData(children, new Position { x = 3f, y = 2f, z = 1f });

// Create a query to select entities based on their components.
var query = manager.CreateQuery(includeTypes: new[] {
	ComponentType.Of<Position>(),
	ComponentType.Of<Velocity>()
});

// Iterate over all tables in all archetypes that match the specified
// component types of the query.
manager.ForEach(query, (table, manager) => {
	// Only the velocities are written, positions can be read-only.
	var velocities = table.GetComponentData<Velocity>();
	var positions  = table.GetComponentDataReadOnly<Position>();
	
	// Set the velocity as two times the position.
	for(int i = 0; i < table.length; ++i)
	{
		velocities[i].x = positions[i].x * 2f;
		velocities[i].y = positions[i].y * 2f;
		velocities[i].z = positions[i].z * 2f;
	}	
});

// Use the universal query to iterate over all entities.
var it = manager.GetIterator(EntityQuery.universal);

while(it.TryNext(out var table))
{
	var entities   = table.GetEntities();
	var positions  = table.GetComponentDataReadOnly<Position>();
	var rotations  = table.GetComponentDataReadOnly<Rotation>();
	var velocities = table.GetComponentDataReadOnly<Velocity>();
	var childOfs   = table.GetComponentDataReadOnly<ChildOf>();
	
	// Print the components.
	for(int i = 0; i < table.length; ++i)
	{
		Console.WriteLine("Entity " + entities[i].index + ":");

		// Check if the table has position components.
		if(positions.Length > 0)
			Console.WriteLine("\t Position: " + positions[i]);
		
		// Check if the table has rotation components.
		if(rotations.Length > 0)
			Console.WriteLine("\t Rotation: " + rotations[i]);

		// Check if the table has velocity components.
		if(velocities.Length > 0)
			Console.WriteLine("\t Velocity: " + velocities[i]);

		// Check if the table has childOf components.
		if(childOfs.Length > 0)
			Console.WriteLine("\t ChildOf: " + childOfs[i]);
	}
}

// Destroy all entities that match the query created above.
manager.DestroyEntity(query);

struct Position
{
	public float x, y, z;

	public override string ToString() => x + ", " + y + ", " + z;
}

struct Rotation
{
	public float x, y, z, w;

	public override string ToString() => x + ", " + y + ", " + z + ", " + w;
}

struct Velocity
{
	public float x, y, z;

	public override string ToString() => x + ", " + y + ", " + z;
}

struct ChildOf
{
	public Entity parent;

	public override string ToString() => "Entity " + parent.index;
}