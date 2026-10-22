using Silk.NET.OpenGL;
using Silk.NET.Windowing;

namespace Szeminarium1
{
    internal static class Program
    {
        private static IWindow graphicWindow;
        private static GL Gl;
        private static uint program;

        // 1  - lekompilal, hiba futaskor
        //private static readonly string VertexShaderSource = @"
        //#version 330 core
        //layout (location = 0) in vec3 vPos;
        //layout (location = 1) in vec4 vCol;
        //out vec4 outCol;
        //void main()
        //{
        //	outCol = vPos; // rossz ertek a szinnek
        //    gl_Position = vec4(vPos.x, vPos.y, vPos.z, 1.0);
        //}";


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

        private static void CheckGLError(string operationTag)
        {
            GLEnum error;
            while ((error = Gl.GetError()) != GLEnum.NoError)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[GL ERROR] Lepes: '{operationTag}' -> Kod: {error} ({(int)error})");
                Console.ResetColor();
            }
        }

        static void Main(string[] args)
        {
            WindowOptions windowOptions = WindowOptions.Default;
            windowOptions.Title = "1. szeminarium - hibakereses es modositas";
            windowOptions.Size = new Silk.NET.Maths.Vector2D<int>(500, 500);

            graphicWindow = Window.Create(windowOptions);

            graphicWindow.Load += GraphicWindow_Load;
            graphicWindow.Update += GraphicWindow_Update;
            graphicWindow.Render += GraphicWindow_Render;

            graphicWindow.Run();
        }

        private static void GraphicWindow_Load()
        {
            Gl = graphicWindow.CreateOpenGL();
            Gl.ClearColor(System.Drawing.Color.White);
            CheckGLError("ClearColor beállítása");

            uint vshader = Gl.CreateShader(ShaderType.VertexShader);
            uint fshader = Gl.CreateShader(ShaderType.FragmentShader);

            Gl.ShaderSource(vshader, VertexShaderSource);
            Gl.CompileShader(vshader);
            

            Gl.GetShader(vshader, ShaderParameterName.CompileStatus, out int vStatus);
            if (vStatus != (int)GLEnum.True)
                throw new Exception("Vertex shader failed to compile: " + Gl.GetShaderInfoLog(vshader));

            Gl.ShaderSource(fshader, FragmentShaderSource);
            // 2 - lekompilal, hiba futaskor
            Gl.CompileShader(fshader); // kihagyni egy shader kompilalasat
            Gl.GetShader(fshader, ShaderParameterName.CompileStatus, out int fStatus);
            if (fStatus != (int)GLEnum.True)
                throw new Exception("Fragment shader failed to compile: " + Gl.GetShaderInfoLog(fshader));

            CheckGLError("Shader forditasok utan");

            program = Gl.CreateProgram();


            // 3 - fekete forma, sok piros error code a console-on
            // rossz sorrend
            //Gl.LinkProgram(program);
            //Gl.AttachShader(program, vshader);
            //Gl.AttachShader(program, fshader);


            // 4 - fekete forma, egy piros error code a console-on
            Gl.AttachShader(program, vshader); // kihagyni egy shader csatolast
            Gl.AttachShader(program, fshader);
            Gl.LinkProgram(program);
            CheckGLError("Program Linkelese");

            Gl.GetProgram(program, GLEnum.LinkStatus, out var status);
            if (status == 0)
            {
                Console.WriteLine($"Error linking shader: {Gl.GetProgramInfoLog(program)}");
            }

            Gl.DetachShader(program, vshader);
            Gl.DetachShader(program, fshader);
            Gl.DeleteShader(vshader);
            Gl.DeleteShader(fshader);
            CheckGLError("Shader takarítás");
        }

        private static void GraphicWindow_Update(double deltaTime)
        {
        }

        private static unsafe void GraphicWindow_Render(double deltaTime)
        {
            Gl.Clear(ClearBufferMask.ColorBufferBit);

            uint vao = Gl.GenVertexArray();
            Gl.BindVertexArray(vao);
            CheckGLError("VAO létrehozás és kötés");

            // 5 - eldeformalt forma
            //float[] vertexArray = new float[] {
            //    -0.5f, -0.5f, 0.0f,
            //    +0.5f, -0.5f, // egyel kevesebb ertek ebben a sorban
            //     0.0f, +0.5f, 0.0f,
            //     1f, 1f, 0f
            //};


            float[] vertexArray = new float[] {
                -0.5f, -0.5f, 0.0f,
                +0.5f, -0.5f, 0.0f,
                 0.0f, +0.5f, 0.0f,
                 1f, 1f, 0f
            };

            float[] colorArray = new float[] {
                1.0f, 0.0f, 0.0f, 1.0f,
                0.0f, 1.0f, 0.0f, 1.0f,
                0.0f, 0.0f, 1.0f, 1.0f,
                1.0f, 0.0f, 0.0f, 1.0f,
            };

            // 6 - eldeformalt forma
            //uint[] indexArray = new uint[] {
            //    0, 1, 2,
            //    2, 1, 99 // nem letezo indexu elem
            //};


            uint[] indexArray = new uint[] {
                0, 1, 2,
                2, 1, 3
            };

            uint vertices = Gl.GenBuffer();

            // 7 - hiba futaskor
            // sorrend felcserelese
            //Gl.BufferData(GLEnum.ArrayBuffer, (ReadOnlySpan<float>)vertexArray.AsSpan(), GLEnum.StaticDraw);
            //Gl.BindBuffer(GLEnum.ArrayBuffer, vertices);

            Gl.BindBuffer(GLEnum.ArrayBuffer, vertices);
            Gl.BufferData(GLEnum.ArrayBuffer, (ReadOnlySpan<float>)vertexArray.AsSpan(), GLEnum.StaticDraw);

            // 8 - hiba futaskor
            // Gl.VertexAttribPointer(0, 5, VertexAttribPointerType.Float, false, 0, null); // hibas egyseg meret
            Gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 0, null);

            // 9 - ures vaszon, sok error a konzolon
            //Gl.EnableVertexAttribArray(999); // hibas attributum ertek

            // 10 - ures vaszon, ures konzol
            // EnableVertexAttribArray kihagyasa
            Gl.EnableVertexAttribArray(0);
            CheckGLError("Vertex puffer konfiguracio");

            uint colors = Gl.GenBuffer();
            Gl.BindBuffer(GLEnum.ArrayBuffer, colors);
            Gl.BufferData(GLEnum.ArrayBuffer, (ReadOnlySpan<float>)colorArray.AsSpan(), GLEnum.StaticDraw);
            Gl.VertexAttribPointer(1, 4, VertexAttribPointerType.Float, false, 0, null);
            Gl.EnableVertexAttribArray(1);
            CheckGLError("Szin puffer konfiguracio");

            uint indices = Gl.GenBuffer();
            Gl.BindBuffer(GLEnum.ElementArrayBuffer, indices);
            Gl.BufferData(GLEnum.ElementArrayBuffer, (ReadOnlySpan<uint>)indexArray.AsSpan(), GLEnum.StaticDraw);
            CheckGLError("Index puffer konfiguracio");

            Gl.BindBuffer(GLEnum.ArrayBuffer, 0);

            // 11 - fekete forma
            // UseProgram kihagyasa
            Gl.UseProgram(program);

            // 12 - ures vaszon
            //Gl.DrawElements(GLEnum.Triangles, (uint)indexArray.Length, GLEnum.UnsignedByte, null); // byte-kent olvasas
            // 13 - ures vaszon, sok error a konzolon
            //Gl.DrawElements(GLEnum.Line, (uint)indexArray.Length, GLEnum.UnsignedInt, null); // vonalak haromszogek 
            Gl.DrawElements(GLEnum.Triangles, (uint)indexArray.Length, GLEnum.UnsignedInt, null);
            CheckGLError("DrawElements kirajzolas");

            Gl.BindBuffer(GLEnum.ElementArrayBuffer, 0);

            Gl.BindVertexArray(vao);

            Gl.DeleteBuffer(vertices);
            Gl.DeleteBuffer(colors);
            Gl.DeleteBuffer(indices);
            Gl.DeleteVertexArray(vao);
            CheckGLError("Takaritas a render vegen");
        }
    }
}