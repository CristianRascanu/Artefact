using Godot;
using System;

public partial class Terrain : MeshInstance3D
{
	//Public export variables that are able to be edited through the inspector.
	[Export] public int Resolution = 128;
	[Export] public float Size = 256f;
	[Export] public float HeightScale = 40f;
	[Export] public float Frequency = 0.01f;
	[Export] public int Octaves = 5;
	[Export] public ulong Seed = 12345; // ulong a longer int

//Ready function runs at runtime
	public override void _Ready(){
		var t = new ImprovedNoise(Seed); // call the ImprovedNoise function with the seed of our choice or the default one.
		for (int i = 0; i < 6; i++) //Loop through 6 axis since it's 3D and not 2D
    		GD.Print($"x={i*0.37:F2} -> {t.Noise(i*0.37, 0.5, 0.3):F4}   z={i*0.37:F2} -> {t.Noise(0.3, 0.5, i*0.37):F4}"); //Print the noise levels of each axis
		var noise = new ImprovedNoise(Seed); // get the noise 
		int s = 256; // the amount of pixels for the image
		var img = Image.CreateEmpty(s, s, false, Image.Format.L8); //Create an empty image variable
		for (int z = 0; z < s; z++) // loop through all the pixels of the image. this is the vertically row
    		for (int x = 0; x < s; x++) // loop through it again. And this is the horizontally so the image is actually 256 by 256
    		{
        		float h = (float)noise.Fbm(x * 0.02, 0.5, z * 0.02, 5); // 
        		img.SetPixel(x, z, new Color(h * 0.5f + 0.5f, 0, 0)); // set the current pixel the colour of the noise on the empty image file
    		}
		img.SavePng("C://Users//fuzzy//Pictures//PerlinNoise//noise_test.png"); // save image to disc
		GD.Print(ProjectSettings.GlobalizePath("C://Users//fuzzy//Pictures//PerlinNoise://noise_test.png"));
		Generate();
	}

	
	public void Generate(){
		var noise = new ImprovedNoise(Seed); // get the noise from Ken Perling's "Improved Noise" function.
		int n = Resolution + 1; // get the normals by using our Resolution  and adding 1.
		float step = Size / Resolution; // getting the step

		var heights = new float [n * n]; // creating a new heights vairable that multiplies the normals together.
		for(int z =0; z < n; z++){ // loop throguh each normal vertically
			for (int x = 0; x < n; x++){ // loop through each normal horizontally
				double wx = GlobalPosition.X + x * step; // get the world x coordinate
				double wz = GlobalPosition.Z + z * step; // get the world z coordinate
				heights[ z * n + x] = (float)noise.Fbm(wx * Frequency, 0.5, wz * Frequency, Octaves) * HeightScale; // apply the fbm(what does it mean and stand for?) function and our octaves to generate the new heights.
			}
		}

		var st = new SurfaceTool(); // Godot's surface tool to create environments, terrain, mmeshes.
		st.Begin(Mesh.PrimitiveType.Triangles); // The primitive that the perlin noise will draw with. Can be quad, in this case it's triangle.

		Vector3 V(int x, int z) => new Vector3(x * step, heights[z * n + x], z * step); // Get the vectors

		void Vert(int x, int z){ // Function to calculate the vertices
			float h = heights[z * n + x] / HeightScale; // divide the heights by the heightscale.
			st.SetColor(h < -0.1f ? new Color(0.76f, 0.70f, 0.50f) // if less then -0.1 set color of normal to green
						: h < 0.35f ? new Color(0.28f, 0.55f, 0.22f) // else set it to  brown?
						: new Color(0.55f, 0.55f, 0.55f)); // else set it to grey
			st.SetUV(new Vector2(x, z) / Resolution); // Set the meshes uv
			st.AddVertex(V(x, z)); // Add the vertices
		}

		for(int z = 0; z < Resolution; z++){ // loop vertically through our resolution input
			for(int x = 0; x < Resolution; x++){ // loop horizontally through our resolution input.
				//clockwise winding for front faces, some use anti-clockwise like Direct 10 & 11.
				//This just means which way the algorithm will begin drawing and how it will be headed.
				Vert(x, z);		Vert(x + 1, z);		Vert(x, z+1);
				Vert(x + 1, z);	Vert(x + 1, z + 1); Vert(x, z + 1);
			}
		}

		st.Index(); // get the index?
		st.GenerateNormals(); // Generate the meshes normals
		Mesh = st.Commit(); // Actually apply all the changes to the mesh.

		MaterialOverride = new StandardMaterial3D{ // create a new standard material that will use albedo colour (allows it to use primitive colours without any texture)
			VertexColorUseAsAlbedo = true,
			Roughness = 1f
		};

		foreach(Node child in GetChildren()) // for each child node remove child if it's a static body.
			if(child is StaticBody3D) child.QueueFree();
		CreateTrimeshCollision(); // allow collision between objects (our player cannot fall throguh the mesh).
	}
}
