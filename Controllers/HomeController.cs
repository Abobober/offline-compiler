using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using CompilerProject.Models;

namespace CompilerProject.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    public ActionResult ExecuteCode()
    {
        return View();
    }


    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }



[HttpPost]
    public ActionResult ExecuteCode(string code, string language)
    {
        var result = RunCode(code, language);
        return View(Tuple.Create(result.Item1, result.Item2));
    }

    private Tuple<string, string> RunCode(string code, string language)
    {
        string output = string.Empty;
        string errors = string.Empty;

        try
        {
            switch (language.ToLower())
            {
                case "python":
                    (output, errors) = ExecutePython(code);
                    break;
                case "cpp":
                    (output, errors) = ExecuteCpp(code);
                    break;
                case "java":
                    (output, errors) = ExecuteJava(code);
                    break;
                case "csharp":
                    (output, errors) = ExecuteCSharp(code);
                    break;
                default:
                    errors = "Unsupported language";
                    break;
            }
        }
        catch (Exception ex)
        {
            errors = ex.Message;
        }

        return Tuple.Create(output, errors);
    }

    private Tuple<string, string> ExecutePython(string code)
    {
        string fileName = "script.py";
        System.IO.File.WriteAllText(fileName, code);

        var psi = new ProcessStartInfo
        {
            FileName = "python",
            Arguments = fileName,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        return ExecuteProcess(psi, fileName);
    }

    private Tuple<string, string> ExecuteCpp(string code)
    {
        string sourceFile = "program.cpp";
        string exeFile = "program.exe";
        System.IO.File.WriteAllText(sourceFile, code);

        var compileProcess = new ProcessStartInfo
        {
            FileName = "g++",
            Arguments = $"{sourceFile} -o {exeFile}",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        var compileResult = ExecuteProcess(compileProcess);

        if (string.IsNullOrEmpty(compileResult.Item2))
        {
            var runProcess = new ProcessStartInfo
            {
                FileName = exeFile,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            var runResult = ExecuteProcess(runProcess);

            System.IO.File.Delete(sourceFile);
            System.IO.File.Delete(exeFile);

            return runResult;
        }

        System.IO.File.Delete(sourceFile);

        return compileResult;
    }

    private Tuple<string, string> ExecuteJava(string code)
    {
        string sourceFile = "Program.java";
        string classFile = "Program.class";
        System.IO.File.WriteAllText(sourceFile, code);

        var compileProcess = new ProcessStartInfo
        {
            FileName = "javac",
            Arguments = sourceFile,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        var compileResult = ExecuteProcess(compileProcess);

        if (string.IsNullOrEmpty(compileResult.Item2))
        {
            var runProcess = new ProcessStartInfo
            {
                FileName = "java",
                Arguments = "Program",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            var runResult = ExecuteProcess(runProcess);

            System.IO.File.Delete(sourceFile);
            System.IO.File.Delete(classFile);

            return runResult;
        }

        System.IO.File.Delete(sourceFile);

        return compileResult;
    }

    private Tuple<string, string> ExecuteCSharp(string code)
    {
        string sourceFile = "Program.cs";
        string exeFile = "Program.exe";
        System.IO.File.WriteAllText(sourceFile, code);

        var compileProcess = new ProcessStartInfo
        {
            FileName = "csc",
            Arguments = $"/out:{exeFile} {sourceFile}",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        var compileResult = ExecuteProcess(compileProcess);

        if (string.IsNullOrEmpty(compileResult.Item2))
        {
            var runProcess = new ProcessStartInfo
            {
                FileName = exeFile,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            var runResult = ExecuteProcess(runProcess);

            System.IO.File.Delete(sourceFile);
            System.IO.File.Delete(exeFile);

            return runResult;
        }

        System.IO.File.Delete(sourceFile);

        return compileResult;
    }

    private Tuple<string, string> ExecuteProcess(ProcessStartInfo psi, string? fileToDelete = null)
    {
        string output = string.Empty;
        string errors = string.Empty;

        try
        {
            using (var process = Process.Start(psi))
            {
                if (process == null)
                {
                    throw new InvalidOperationException("Failed to start process.");
                }

                output = process.StandardOutput.ReadToEnd();
                errors = process.StandardError.ReadToEnd();
                process.WaitForExit();
            }
        }
        catch (Exception ex)
        {
            errors = ex.Message;
        }
        finally
        {
            if (!string.IsNullOrEmpty(fileToDelete) && System.IO.File.Exists(fileToDelete))
            {
                System.IO.File.Delete(fileToDelete);
            }
        }

        return Tuple.Create(output, errors);
    }
}




