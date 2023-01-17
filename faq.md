# FAQ

> How do I create an entity?

Use the `EntityManager.CreateEntity` variants. For example, to create an entity without components:
```CSharp
var entity = manager.CreateEntity();
```

> How do I create an entity with components?

There are two ways: The first one is to add the components to an already existing entity:
```CSharp
var entity = manager.CreateEntity();
manager.AddComponent(entity, new Position(1f, 2f, 3f));
manager.AddComponent(entity, new Rotation(4f, 3f, 2f, 1f));
```
The second way is to use the archetype to create entities with a specific set of components:
```CSharp
var archetype = manager.CreateArchetype(new[] {
	ComponentType.Of<Position>(),
	ComponentType.Of<Rotation>()
});
var entity = manager.CreateEntity(archetype);
```
Prefer the second variant for new entities. It requires fewer operations
and is generally faster than adding components to an entity, as adding components first
requires moving the entity into a new archetype (with the added component).

> How do I create multiple entities?

To create entities for later use, allocate a buffer into which the entities are to be copied.
The size of the buffer determines the number of entities:
```CSharp
var archetype = manager.CreateArchetype(new[] {
	ComponentType.Of<Position>(),
	ComponentType.Of<Rotation>()
});
var entities = new Entity[10];
manager.CreateEntity(archetype, entities);
```
Note that stack allocation is also supported:
```CSharp
Span<Entity> temp = stackalloc Entity[10];
manager.CreateEntity(archetype, temp);
```

> How to I destroy an entity?

Destroy an single entity with:
```CSharp
var entity = manager.CreateEntity();

manager.DestroyEntity(entity)
```
Or an overload can be used to destroy multiple entities:
```CSharp
var entites = new Entity[10];
manager.CreateEntity(entities);

manager.DestroyEntities(entities);
```

> How do I check if an entity exists?

To check if a single entity exists use:
```CSharp
bool exists = manager.Exists(entity);
```


> How do I create an archetype?

Use the `EntityManager.CreateArchetype` variants. Or use the boolean operations like
`EntityManager.CombineArchetypes`:
```CSharp
var archetype1 = manager.CreateArchetype(new[] {
	ComponentType.Of<Position>(),
	ComponentType.Of<Rotation>()
});
var archetype2 = manager.CreateArchetype(new[] {
	ComponentType.Of<Position>(),
	ComponentType.Of<Velocity>()
});
var archetype3 = manager.CombineArchetypes(archetype1, archetype2);
// = (Position, Rotation, Velocity)
```
Note that stack allocation for the component types is also supported:
```CSharp
Span<ComponentType> types = stackalloc ComponentType[] {
	ComponentType.Of<Position>(),
	ComponentType.Of<Rotation>()
};
var archetype = manager.CreateArchetype(types);
```

> How do I get all entities of an archetype?

The simplest variant is to copy the entities into a buffer:
```CSharp
var archetype = manager.CreateArchetype(new[] {
	ComponentType.Of<Position>(),
	ComponentType.Of<Rotation>()
});

var count = manager.GetEntityCount(archetype);
var entities = new Entity[count];

manager.GetEntities(archetype, entities);
```

> How do I get the value of an component?

The simplest variant is to simply get the value without checking if the
entity has the component (in this case the returned value is the default value):
```CSharp
var position = manager.GetComponentData<Position>(entity);
```
To also check if the entity has the component use:
```CSharp
if(manager.TryGetComponent(entity, out Position position))
{
	// ...
}
```

> How do I set the value of an component?

Just pass in the value:
```CSharp
var position = new Position(1f, 2f, 3f);
bool success = manager.SetComponentData(entity, in position);

// Short-hand
manager.SetComponentData(entity, new Rotation(4f, 3f, 2f, 1f)); 
```

> How do I get all entities with a specific set of components?

Queries allow to filter entities based on their archetype via a selection of included and
excluded component types:
```CSharp
var query = manager.CreateQuery(includeTypes: new[] {
	ComponentType.Of<Position>(),
	ComponentType.Of<Rotation>()
}, excludeTypes: new[] {
	ComponentType.Of<Scale>()
});

var count = manager.GetEntityCount(query);
var entities = new Entity[count];

manager.GetEntities(query, entities);
```

> How do I run systems or functions on entities?

Basically, there are three different ways to run systems on entities, but only two of them really scale efficiently.
The simplest variant is to set the component values via the entity:
```CSharp
var archetype = manager.CreateArchetype(new[] {
	ComponentType.Of<Position>(),
	ComponentType.Of<Rotation>()
});
var entities = new Entity[10];

manager.CreateEntity(archetype, entities);

for(int i = 0; i < entities.Length; ++i)
{
	var position = manager.GetComponent<Position>(entities[i]);
	var rotation = new Rotation(position.x, position.y, position.z, i);
	manager.SetComponentData(entities[i], in rotation);
}
```
If the same value is used for all entities, the operation can be batched
(which leads to a speed-up):
```CSharp
manager.SetComponentData(entities, new Position(1f, 2f, 3f));
```
But this variant does not scale well because it is not guaranteed that the entities are in a continuous chunk of memory.
Furthermore, in the general case, a system should always run on all entities that meet the system's conditions.
The next simpler variant is to run a system on all entities of an archetype or query:
```CSharp
var query = manager.CreateQuery(includeTypes: new[] {
	ComponentType.Of<Position>(),
	ComponentType.Of<Rotation>()
});

manager.ForEach(query, static (table, _) => {
	var positions = table.GetComponentData<Rotation>();
	var rotations = table.GetComponentDataReadOnly<Position>();

	for(int i = 0; i < table.length; ++i)
		rotations[i] = new Rotation(positions[i].x, positions[i].y, positions[i].z, i);
});
```
Use static methods if possible to avoid triggering the GC.

The third variant is to manually iterate over all tables of an archetype or query with an `EntityIterator` (similar to the second variant):
```CSharp
using var it = manager.GetIterator(query);

while(it.TryNext(out var table))
{
	var positions = table.GetComponentData<Rotation>();
	var rotations = table.GetComponentDataReadOnly<Position>();

	for(int i = 0; i < table.length; ++i)
		rotations[i] = new Rotation(positions[i].x, positions[i].y, positions[i].z, i);
}
```

> How do I create/destroy entities in a iterator/foreach loop?

If entities in the same archetype as the iterator are added or destroyed, an exception
occurs because the underlying memory layout may have changed.
To avoid this problem defer the creation or destruction with the `EntityCommandBuffer` which can record commands and play them back at a later time:
```CSharp
using var cmd = manager.CreateCommandBuffer();

manager.ForEach(query, (table, cmd) => {
	// ...
	cmd.CreateEntity(archetype);
}, cmd)

manager.Playback(cmd);
```

> How do I create an entity template or prefab with predefined component values that I can instantiate any time?

Instead of setting the component values of an living entity in the manager use
the `EntityPrefab` which will only store the component types and values (no entity is created).
If the component type is not in the prefab, it will be added automatically:
```CSharp
var prefab = new EntityPrefab();

prefab.SetComponentData(new Position(1f, 2f, 3f));
prefab.SetComponentData(new Rotation(1f, 2f, 3f, 4f));

using var manager = new EntityManager();
var entities = new Entity[10];

manager.Instantiate(prefab, entities);
```
It is also possible to preallocate the prefab with multiple component types at once
(similar to the creation of archetypes):
```CSharp
var prefab = new EntityPrefab(new[] {
	ComponentType.Of<Position>(),
	ComponentType.Of<Rotation>(),
	ComponentType.Of<Velocity>()
});

prefab.SetComponentData(new Velocity(1f, 2f, 3f));
```
A fluent API is also available:
```CSharp
var prefab = new EntityPrefab()
	.WithComponentData<Position>()
	.WithComponentData<Rotation>(new Rotation(1f, 2f, 3f, 4f));
```

> How do I convert an entity back to a prefab?

Create a new prefab from an already existing entity:
```CSharp
var prefab = manager.ToPrefab(entity);

if(prefab != null)
{
	prefab.SetComponentData<Position>(new Position(1f, 2f, 3f));
	// ...
}
```