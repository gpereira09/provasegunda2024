using Microsoft.EntityFrameworkCore;
using API.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDataContext>();

var app = builder.Build();

app.MapGet("/", () => "provasegunda2024");

app.MapPost("/api/aluno/cadastrar", async (Aluno aluno, AppDataContext context) =>
{
    context.Alunos.Add(aluno);
    await context.SaveChangesAsync();
    return Results.Created($"/api/aluno/{aluno.Id}", aluno);
});

app.MapPost("/api/imc/cadastrar", async (HttpContext context, AppDataContext db) =>
{
    var imc = await context.Request.ReadFromJsonAsync<IMC>();
    if (imc == null)
    {
        return Results.BadRequest("Dados inválidos.");
    }

    var aluno = await db.Alunos.FindAsync(imc.AlunoId);
    if (aluno == null)
    {
        return Results.NotFound($"Aluno com ID {imc.AlunoId} não encontrado.");
    }

    var imcCalculado = CalcularImc(imc.Peso, imc.Altura);
    imc.Classificacao = ClassificarImc(imcCalculado);
    imc.ImcCalculado = imcCalculado;

    db.IMCs.Add(imc);
    await db.SaveChangesAsync();

    return Results.Created($"/api/imc/{imc.Id}", imc);
});

double CalcularImc(double peso, double altura) => peso / (altura * altura);

string ClassificarImc(double imc)
{
    return imc switch
    {
        < 18.5 => "Magro",
        >= 18.5 and < 25 => "Normal",
        >= 25 and < 30 => "Sobpeso",
        >= 30 and < 40 => "Obesidade",
        _ => "Obesidade Grave"
    };
}

app.MapGet("/api/imc/listar", async (AppDataContext context) =>
{
    return await context.IMCs.ToListAsync();
});

app.MapGet("/api/aluno/listar", async (AppDataContext context) =>
{
    return await context.Alunos.ToListAsync();
});

app.MapPut("/api/imc/atualizar/{id}", async (int id, HttpContext context, AppDataContext db) =>
{

    var imcExistente = await db.IMCs.FindAsync(id);
    if (imcExistente == null)
    {
        return Results.NotFound($"IMC com ID {id} não encontrado.");
    }

    var imcAlterado = await context.Request.ReadFromJsonAsync<IMC>();
    if (imcAlterado == null)
    {
        return Results.BadRequest("Dados de IMC inválidos.");
    }

  
    var aluno = await db.Alunos.FindAsync(imcAlterado.AlunoId);
    if (aluno == null)
    {
        return Results.NotFound($"Aluno com ID {imcAlterado.AlunoId} não encontrado.");
    }

    imcExistente.Peso = imcAlterado.Peso;
    imcExistente.Altura = imcAlterado.Altura;
    imcExistente.Classificacao = ClassificarImc(CalcularImc(imcAlterado.Peso, imcAlterado.Altura));
    imcExistente.ImcCalculado = CalcularImc(imcAlterado.Peso, imcAlterado.Altura);
    imcExistente.AlunoId = imcAlterado.AlunoId;

    db.IMCs.Update(imcExistente);
    await db.SaveChangesAsync();

    return Results.Ok(imcExistente);
});



app.Run();
