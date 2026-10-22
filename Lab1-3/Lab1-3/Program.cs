using Silk.NET.OpenGL;
using Silk.NET.Windowing;

namespace Szeminarium1
{
    internal static class Program
    {
        private static IWindow graphicWindow;

        private static GL Gl;

        private static uint program;

        private static readonly string VertexShaderSource = @"
        #version 330 core
        layout (location = 0) in vec3 vPos;
		layout (location = 1) in vec4 vCol;

		out vec4 outCol;
        
        void main()
        {
			outCol = vCol;
            gl_Position = vec4(vPos.x, vPos.y, vPos.z, 1.0);
        }
        ";


        private static readonly string FragmentShaderSource = @"
        #version 330 core
        out vec4 FragColor;
		
		in vec4 outCol;

        void main()
        {
            FragColor = outCol;
        }
        ";

        static void Main(string[] args)
        {
            WindowOptions windowOptions = WindowOptions.Default;
            windowOptions.Title = "Lab1-2 - kamu kocka";
            windowOptions.Size = new Silk.NET.Maths.Vector2D<int>(500, 500);

            graphicWindow = Window.Create(windowOptions);

            graphicWindow.Load += GraphicWindow_Load;
            graphicWindow.Update += GraphicWindow_Update;
            graphicWindow.Render += GraphicWindow_Render;

            graphicWindow.Run();
        }

        private static void GraphicWindow_Load()
        {
            // egszeri beallitasokat
            //Console.WriteLine("Loaded");

            Gl = graphicWindow.CreateOpenGL();

            Gl.ClearColor(System.Drawing.Color.White);

            uint vshader = Gl.CreateShader(ShaderType.VertexShader);
            uint fshader = Gl.CreateShader(ShaderType.FragmentShader);

            Gl.ShaderSource(vshader, VertexShaderSource);
            Gl.CompileShader(vshader);
            Gl.GetShader(vshader, ShaderParameterName.CompileStatus, out int vStatus);
            if (vStatus != (int)GLEnum.True)
                throw new Exception("Vertex shader failed to compile: " + Gl.GetShaderInfoLog(vshader));

            Gl.ShaderSource(fshader, FragmentShaderSource);
            Gl.CompileShader(fshader);

            program = Gl.CreateProgram();
            Gl.AttachShader(program, vshader);
            Gl.AttachShader(program, fshader);
            Gl.LinkProgram(program);
            Gl.DetachShader(program, vshader);
            Gl.DetachShader(program, fshader);
            Gl.DeleteShader(vshader);
            Gl.DeleteShader(fshader);

            Gl.GetProgram(program, GLEnum.LinkStatus, out var status);
            if (status == 0)
            {
                Console.WriteLine($"Error linking shader {Gl.GetProgramInfoLog(program)}");
            }

        }

        private static void GraphicWindow_Update(double deltaTime)
        {
            // NO GL
            // make it threadsave
            //Console.WriteLine($"Update after {deltaTime} [s]");
        }

        private static float[] GenerateAllFacesCutVertices(int dim = 3)
        {
            int pontokPerLap = (dim + 1) * (dim + 1);
            float[] allVertices = new float[3 * pontokPerLap * 3];

            // A kozeppontbol kifuto 3 fo elvektor
            float[] vBal = new float[] { -4f / 6f, 2f / 6f, 0f };
            float[] vJobb = new float[] { 4f / 6f, 2f / 6f, 0f };
            float[] vLe = new float[] { 0f, -5f / 6f, 0f };

            // A 3 lapot kifeszíto vektor-parok: [U, V]
            // 0. lap: Bal oldal (vBal, vLe)
            // 1. lap: Jobb oldal (vJobb, vLe)
            // 2. lap: Felso oldal (vBal, vJobb)
            float[][] faceBasis = new float[][] {
                new float[] { vBal[0], vBal[1], vBal[2], vLe[0], vLe[1], vLe[2] },
                new float[] { vJobb[0], vJobb[1], vJobb[2], vLe[0], vLe[1], vLe[2] },
                new float[] { vBal[0], vBal[1], vBal[2], vJobb[0], vJobb[1], vJobb[2] }
            };

            for (int face = 0; face < 3; face++)
            {
                float ux = faceBasis[face][0];
                float uy = faceBasis[face][1];
                float uz = faceBasis[face][2];

                float vx = faceBasis[face][3];
                float vy = faceBasis[face][4];
                float vz = faceBasis[face][5];

                int faceVertexOffset = face * pontokPerLap;

                for (int i = 0; i <= dim; i++)
                {
                    float uSuly = (float)i / dim;

                    for (int j = 0; j <= dim; j++)
                    {
                        float vSuly = (float)j / dim;

                        // A pont abszolut indexe az 1D tombben
                        int pointIndex = faceVertexOffset + (i * (dim + 1) + j);
                        int baseIndex = pointIndex * 3;

                        // Kezdopont (0, 0, 0) + uSuly * U + vSuly * V
                        allVertices[baseIndex + 0] = uSuly * ux + vSuly * vx;
                        allVertices[baseIndex + 1] = uSuly * uy + vSuly * vy;
                        allVertices[baseIndex + 2] = uSuly * uz + vSuly * vz;
                    }
                }
            }

            return allVertices;
        }

        private static uint[] GenerateGridLineIndices(int dim = 3)
        {
            int pontokPerSor = dim + 1;
            int pontokPerLap = pontokPerSor * pontokPerSor;

            // Minden vonal 2 csucsindexbol all (kezdet, veg)
            int vonalakPerLap = 2 * (dim - 1);
            uint[] lineIndices = new uint[3 * vonalakPerLap * 2];

            int writeIdx = 0;

            for (int face = 0; face < 3; face++)
            {
                uint faceOffset = (uint)(face * pontokPerLap);

                for (int i = 1; i < dim; i++)
                {
                    uint startPoint = faceOffset + (uint)(i * pontokPerSor + 0);
                    uint endPoint = faceOffset + (uint)(i * pontokPerSor + dim);

                    lineIndices[writeIdx++] = startPoint;
                    lineIndices[writeIdx++] = endPoint;
                }

                for (int j = 1; j < dim; j++)
                {
                    uint startPoint = faceOffset + (uint)(0 * pontokPerSor + j);
                    uint endPoint = faceOffset + (uint)(dim * pontokPerSor + j);

                    lineIndices[writeIdx++] = startPoint;
                    lineIndices[writeIdx++] = endPoint;
                }
            }

            return lineIndices;
        }

        private static unsafe void GraphicWindow_Render(double deltaTime)
        {
            //Console.WriteLine($"Render after {deltaTime} [s]");

            Gl.Clear(ClearBufferMask.ColorBufferBit);

            uint vao = Gl.GenVertexArray();
            Gl.BindVertexArray(vao);

            float[] vertexArray = new float[] {
                // bal oldal
                0f, 0f, 0f, // 0
                -4/6f, 2/6f, 0f, // 1
                -4/6f, -3/6f, 0f, // 2
                0f, -5/6f, 0f, // 3
                // jobb oldal
                0f, 0f, 0f, // 4
                0f, -5/6f, 0f, // 5
                4/6f, -3/6f, 0f, // 6
                4/6f, 2/6f, 0f, // 7
                // felso oldal
                0f, 0f, 0f, // 8
                4/6f, 2/6f, 0f, // 9
                0f, 4/6f, 0f, // 10
                -4/6f, 2/6f, 0f // 11
            };

            float[] colorArray = new float[] {
                // bal oldal
                1.0f, 0.0f, 0.0f, 1.0f,
                1.0f, 0.0f, 0.0f, 1.0f,
                1.0f, 0.0f, 0.0f, 1.0f,
                1.0f, 0.0f, 0.0f, 1.0f,
                // jobb oldal
                0.0f, 1.0f, 0.0f, 1.0f,
                0.0f, 1.0f, 0.0f, 1.0f,
                0.0f, 1.0f, 0.0f, 1.0f,
                0.0f, 1.0f, 0.0f, 1.0f,
                // felso oldal
                0.0f, 0.0f, 1.0f, 1.0f,
                0.0f, 0.0f, 1.0f, 1.0f,
                0.0f, 0.0f, 1.0f, 1.0f,
                0.0f, 0.0f, 1.0f, 1.0f,
            };

            uint[] indexArray = new uint[] {
                // bal oldal
                0, 1, 3,
                3, 1, 2,
                // jobb oldal
                4, 5, 6,
                4, 6, 7,
                // felso oldal
                8, 9, 10,
                8, 10, 11
            };

            uint vertices = Gl.GenBuffer();
            Gl.BindBuffer(GLEnum.ArrayBuffer, vertices);
            Gl.BufferData(GLEnum.ArrayBuffer, (ReadOnlySpan<float>)vertexArray.AsSpan(), GLEnum.StaticDraw);
            Gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 0, null);
            Gl.EnableVertexAttribArray(0);

            uint colors = Gl.GenBuffer();
            Gl.BindBuffer(GLEnum.ArrayBuffer, colors);
            Gl.BufferData(GLEnum.ArrayBuffer, (ReadOnlySpan<float>)colorArray.AsSpan(), GLEnum.StaticDraw);
            Gl.VertexAttribPointer(1, 4, VertexAttribPointerType.Float, false, 0, null);
            Gl.EnableVertexAttribArray(1);

            uint indices = Gl.GenBuffer();
            Gl.BindBuffer(GLEnum.ElementArrayBuffer, indices);
            Gl.BufferData(GLEnum.ElementArrayBuffer, (ReadOnlySpan<uint>)indexArray.AsSpan(), GLEnum.StaticDraw);

            Gl.BindBuffer(GLEnum.ArrayBuffer, 0);

            Gl.UseProgram(program);

            Gl.DrawElements(GLEnum.Triangles, (uint)indexArray.Length, GLEnum.UnsignedInt, null); // we used element buffer
            Gl.BindBuffer(GLEnum.ElementArrayBuffer, 0);
            Gl.BindVertexArray(vao);

            // always unbound the vertex buffer first, so no halfway results are displayed by accident
            Gl.DeleteBuffer(vertices);
            Gl.DeleteBuffer(colors);
            Gl.DeleteBuffer(indices);
            Gl.DeleteVertexArray(vao);


            int dim = 3;

            float[] cutArray = GenerateAllFacesCutVertices(dim);
            float[] curColorArray = new float[(cutArray.Length / 3) * 4];
            uint[] cutIndexArray = GenerateGridLineIndices(dim);

            for (int i = 0; i < cutArray.Length / 3; i++)
            {
                curColorArray[i * 4 + 3] = 1.0f;
            }
            uint lineVao = Gl.GenVertexArray();
            Gl.BindVertexArray(lineVao);

            uint lineVertices = Gl.GenBuffer();
            Gl.BindBuffer(GLEnum.ArrayBuffer, lineVertices);
            Gl.BufferData(GLEnum.ArrayBuffer, (ReadOnlySpan<float>)cutArray.AsSpan(), GLEnum.StaticDraw);
            Gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 0, null);
            Gl.EnableVertexAttribArray(0);

            uint lineColors = Gl.GenBuffer();
            Gl.BindBuffer(GLEnum.ArrayBuffer, lineColors);
            Gl.BufferData(GLEnum.ArrayBuffer, (ReadOnlySpan<float>)curColorArray.AsSpan(), GLEnum.StaticDraw);
            Gl.VertexAttribPointer(1, 4, VertexAttribPointerType.Float, false, 0, null);
            Gl.EnableVertexAttribArray(1);

            uint lineIndices = Gl.GenBuffer();
            Gl.BindBuffer(GLEnum.ElementArrayBuffer, lineIndices);
            Gl.BufferData(GLEnum.ElementArrayBuffer, (ReadOnlySpan<uint>)cutIndexArray.AsSpan(), GLEnum.StaticDraw);

            Gl.BindBuffer(GLEnum.ArrayBuffer, 0);

            Gl.UseProgram(program);

            Gl.DrawElements(GLEnum.Lines, (uint)cutIndexArray.Length, GLEnum.UnsignedInt, null);
            Gl.BindBuffer(GLEnum.ElementArrayBuffer, 0);
            Gl.BindVertexArray(0);

            Gl.DeleteBuffer(lineVertices);
            Gl.DeleteBuffer(lineColors);
            Gl.DeleteBuffer(lineIndices);
            Gl.DeleteVertexArray(lineVao);
        }
    }
}
