using System;
using System.IO;
using System.Text.Json;
using Checs;

using var manager = new EntityManager();

// Setup archetypes and entities.
var playerArchetype = manager.CreateArchetype(
	ComponentType.Of<Position>(),
	ComponentType.Of<Velocity>(),
	ComponentType.Of<Health>(),
	ComponentType.Of<Player>()
);

var enemyArchetype = manager.CreateArchetype(
	ComponentType.Of<Position>(),
	ComponentType.Of<Velocity>(),
	ComponentType.Of<Health>(),
	ComponentType.Of<Enemy>()
);

var bulletArchetype = manager.CreateArchetype(
	ComponentType.Of<Position>(),
	ComponentType.Of<Velocity>(),
	ComponentType.Of<Bullet>()
);

var player = manager.CreateEntity(playerArchetype);

var enemies = new Entity[20];
manager.CreateEntity(enemyArchetype, enemies);

var bullets = new Entity[30];
manager.CreateEntity(bulletArchetype, bullets);

// Generate random data.
var random = new Random();

manager.SetComponentData(player, new Position { x = 1f, y = 2f });
manager.SetComponentData(player, new Velocity { x = 0f, y = 0f });
manager.SetComponentData(player, new Health { value = random.Next() % 100, max = 100 });
manager.SetComponentData(player, new Player { score = random.Next() % 1000 });

manager.ForEach(enemies, (table, random) => {
	var positions = table.GetComponentData<Position>();
	var velocites = table.GetComponentData<Velocity>();
	var healths   = table.GetComponentData<Health>();

	for(int i = 0; i < table.length; ++i)
	{
		positions[i] = new Position { x = random.NextSingle(), y = random.NextSingle() };
		velocites[i] = new Velocity { x = random.NextSingle(), y = random.NextSingle() };
		healths[i]   = new Health { value = random.Next() % 50, max = 50 + random.Next() % 100 };
	}
}, random);

manager.ForEach(bullets, (table, random) => {
	var positions = table.GetComponentData<Position>();
	var velocites = table.GetComponentData<Velocity>();

	for(int i = 0; i < table.length; ++i)
	{
		positions[i] = new Position { x = random.NextSingle(), y = random.NextSingle() };
		velocites[i] = new Velocity { x = random.NextSingle(), y = random.NextSingle() };
	}
}, random);

// Serialize to JSON.
static void Serialize(EntityManager manager, string filename, JsonSerializerOptions options)
{
	using var file = File.Open(filename, FileMode.Create);
	using var json = new Utf8JsonWriter(file, new JsonWriterOptions { Indented = true });

	var it = manager.GetIterator(EntityQuery.universal);

	json.WriteStartArray();

	while(it.TryNext(out var table))
	{
		var arrays = new Array[table.typeCount];
		var types  = new ComponentType[table.typeCount];

		table.GetComponentTypes(types);

		for(int i = 0; i < types.Length; ++i)
			arrays[i] = table.ToArray(types[i]);

		for(int i = 0; i < table.length; ++i)
		{
			json.WriteStartObject();

			// Convert structure-of-arrays to array-of-structures.
			for(int t = 0; t < types.Length; ++t)
			{
				var type  = types[t].ToType();
				var value = JsonSerializer.Serialize(arrays[t].GetValue(i), options);

				json.WritePropertyName(type.FullName);
				json.WriteRawValue(value);
			}

			json.WriteEndObject();
		}
	}

	json.WriteEndArray();
}

static void Deserialize(EntityManager manager, string filename, JsonSerializerOptions options)
{
	var document = JsonDocument.Parse(File.ReadAllText("world.json"));

	var entityTypeName = typeof(Entity).FullName;

	foreach(var obj in document.RootElement.EnumerateArray())
	{
		// Get the entity at the start to set the components directly.
		var entity = obj.GetProperty(entityTypeName).Deserialize<Entity>(options);

		foreach(var prop in obj.EnumerateObject())
		{
			if(prop.Name == entityTypeName)
				continue;

			var type  = Type.GetType(prop.Name);
			var value = prop.Value.Deserialize(type, options);
			
			manager.WriteComponentData(entity, value);
		}
	}
}

var options = new JsonSerializerOptions
{
	IncludeFields = true,
	IgnoreReadOnlyProperties = true
};

var sw = new System.Diagnostics.Stopwatch();
sw.Start();

Serialize(manager, "world.json", options);
Console.WriteLine("Completed serialization (" + sw.Elapsed.TotalMilliseconds + "ms).");

sw.Restart();

Deserialize(manager, "world.json", options);
Console.WriteLine("Completed deserialization (" + sw.Elapsed.TotalMilliseconds + "ms).");

struct Position
{
	public float x, y;

	public override string ToString() => $"<{x},{y}>";
}

struct Velocity
{
	public float x, y;

	public override string ToString() => $"<{x},{y}>";
}

struct Health
{
	public int value, max;

	public override string ToString() => $"{value}/{max}";
}

struct Player
{
	public int score;

	public override string ToString() => $"{score}pt";
}

struct Enemy
{
}

struct Bullet
{
}