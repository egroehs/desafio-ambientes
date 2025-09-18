using Microsoft.AspNetCore.Diagnostics;

class Program
{
    public static void Main(string[] args)
    {
        var appBuilder = WebApplication.CreateBuilder(args);

        appBuilder.Services.AddEndpointsApiExplorer();
        appBuilder.Services.AddSwaggerGen();

        var app = appBuilder.Build();

        app.UseHttpsRedirection();
        app.UseSwagger();
        app.UseSwaggerUI();

        /*
        * A aplicação não pode detalhar o erro em ambiente de produção
        * Ao invés disso, em produção deve fazer "handling" para o endpoint de erro
        */
        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        } else
        {
            app.UseExceptionHandler("/error");
        }
        

        app.MapGet(
          "/error-map",
          (int intent) =>
          {
              throw intent switch
              {
                  400 => new Exception("Desenvolvedor: A aplicação gerou uma exceção visando o status [400]"),
                  404 => new Exception("Desenvolvedor: A aplicação gerou uma exceção visando o status [404]"),
                  502 => new Exception("Desenvolvedor: A aplicação gerou uma exceção visando o status [502]"),
                  _ => new Exception("Desenvolvedor: A aplicação não conseguiu mapear a intenção de status. Então gerou uma exceção para o status [500]"),
              };
          }
        );

        app.MapGet(
          "/error",
          (HttpContext context) =>
          {
              var contextException = context.Features.Get<IExceptionHandlerFeature>();

              if (contextException == null)
                  return Results.Problem(statusCode: 501);

              /*
              * Esse ok não atende às necessidades do desafio.
              * Implemente aqui a lógica de mapeamento que faz com que seja devolvida
              * uma resposta com o status code equivalente ao código de erro contido
              * dentro da mensagem da exceção lançada (dica: analise o "contextException")
              */
              var message = contextException.Error.Message;

              var statusCode = 500;
              var startIndex = message.IndexOf("[");
              var endIndex = message.IndexOf("]");
              if (startIndex >= 0 && endIndex > startIndex)
              {
                  var codeStr = message.Substring(startIndex + 1, endIndex - startIndex - 1);
                  if (int.TryParse(codeStr, out var parsed))
                  {
                      statusCode = parsed;
                  }
              }

              return Results.Problem(
                title: "Ocorreu um erro ao processar sua solicitação.",
                statusCode: statusCode
              );
          }).ExcludeFromDescription();

        app.Run();
    }
}