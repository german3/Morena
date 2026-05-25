using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.AI;

namespace Morena.Services;

public class MockChatClient : IChatClient
{
    private readonly Random _random = new();

    public ChatClientMetadata Metadata { get; } = new("MockChatClient");

    public Task<ChatResponse> GetResponseAsync(
        IEnumerable<ChatMessage> chatMessages,
        ChatOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var lastMessage = chatMessages.LastOrDefault(m => m.Role == ChatRole.User)?.Text ?? "";

        // Check if this is a request for suggestions
        if (lastMessage.Contains("Suggest up to 3 follow-up") || lastMessage.Contains("suggestions"))
        {
            var suggestions = new[]
            {
                "¿Cómo pago mi impuesto predial?",
                "¿Cuáles son los requisitos de la Beca?",
                "¿Cómo tramitar la cartilla militar?"
            };
            var json = JsonSerializer.Serialize(suggestions);
            var responseMsg = new ChatMessage(ChatRole.Assistant, json);
            return Task.FromResult(new ChatResponse(responseMsg));
        }

        var replyText = GetAmloReply(lastMessage);
        var replyMsg = new ChatMessage(ChatRole.Assistant, replyText);
        return Task.FromResult(new ChatResponse(replyMsg));
    }

    public async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
        IEnumerable<ChatMessage> chatMessages,
        ChatOptions? options = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var lastMessage = chatMessages.LastOrDefault(m => m.Role == ChatRole.User)?.Text ?? "";
        var replyText = GetAmloReply(lastMessage);

        // We stream the reply character by character or word by word to make it realistic
        var words = replyText.Split(' ');
        for (int i = 0; i < words.Length; i++)
        {
            if (cancellationToken.IsCancellationRequested) yield break;

            var chunk = words[i] + (i == words.Length - 1 ? "" : " ");
            yield return new ChatResponseUpdate
            {
                Role = ChatRole.Assistant,
                Contents = [new TextContent(chunk)]
            };

            // Simulate typing speed
            await Task.Delay(_random.Next(30, 80), cancellationToken);
        }
    }

    private string GetAmloReply(string query)
    {
        query = query.ToLowerInvariant();

        string greeting = "Miren, ciudadanos de Reynosa... ";
        if (_random.Next(2) == 0)
        {
            greeting = "Bueno, pues... Qué gusto saludarlos. Miren, en el gobierno de la transformación de Reynosa, Tamaulipas... ";
        }

        if (query.Contains("predial") || query.Contains("catastro") || query.Contains("propiedad"))
        {
            return greeting + @"Sobre el **Impuesto Predial** en Reynosa, les tengo una buena noticia:
- Hay un **descuento del 15%** si pagan temprano en enero y febrero.
- Y para nuestros adultos mayores, jubilados, pensionados y personas con discapacidad, ¡el **50% de descuento**! Eso es apoyar al pueblo bueno.
- **Requisitos:** Sólo necesitan su clave catastral, una copia de su identificación oficial (INE) y el recibo del año anterior.
- **¿Dónde pagar?** Pueden acudir al Palacio Municipal de Reynosa, de lunes a viernes, o hacerlo directamente en el portal web oficial del ayuntamiento, sin intermediarios. ¡Se acabó la corrupción y el influyentismo!
<citation filename='Tramites_Reynosa.md'>descuento del 15% en enero y febrero</citation>
<citation filename='Tramites_Reynosa.md'>50% de descuento para pensionados</citation>";
        }

        if (query.Contains("beca") || query.Contains("estudio") || query.Contains("escuela") || query.Contains("apoyo"))
        {
            return greeting + @"El tema de las **Becas Municipales de Reynosa** es prioritario. Queremos que nuestros jóvenes estudien, no queremos que caigan en la delincuencia. Abrazos, no balazos.
- **Requisitos:** Se necesita la constancia de estudios vigente, la boleta de calificaciones con promedio mínimo de 8.0, el CURP del estudiante y el INE del padre o tutor (con dirección en Reynosa).
- **Proceso:** La convocatoria se abre anualmente y el registro es 100% en línea en el portal oficial del Municipio. Es directo, para que no haya 'moches' ni intermediarios que se queden con el dinero de las familias.
- **Pago:** El apoyo se deposita directamente en una tarjeta bancaria para el estudiante. ¡Me canso ganso que vamos a apoyar la educación!
<citation filename='Tramites_Reynosa.md'>promedio mínimo de 8.0</citation>
<citation filename='Tramites_Reynosa.md'>registro es 100% en línea</citation>";
        }

        if (query.Contains("acta") || query.Contains("nacimiento") || query.Contains("registro civil"))
        {
            return greeting + @"Para tramitar una copia certificada de su **Acta de Nacimiento** en Reynosa:
- Pueden acudir a las oficinas del **Registro Civil** ubicadas en la zona centro de la ciudad.
- O bien, lo pueden hacer por internet las 24 horas del día. Es muy rápido.
- **Costo:** Aproximadamente $105 pesos.
- **Requisitos:** Conocer su CURP o sus datos registrales completos (año, libro, acta). 
Miren, esto es parte de la simplificación administrativa para facilitarle la vida a la gente.
<citation filename='Tramites_Reynosa.md'>oficinas del Registro Civil</citation>
<citation filename='Tramites_Reynosa.md'>costo es de $105 pesos</citation>";
        }

        if (query.Contains("funcionamiento") || query.Contains("licencia de negocio") || query.Contains("comercio") || query.Contains("negocio"))
        {
            return greeting + @"Para los emprendedores y comerciantes que quieren su **Licencia de Funcionamiento**:
- **Requisitos:** RFC de la empresa o persona física, identificación oficial, comprobante de domicilio comercial en Reynosa, dictamen de Uso de Suelo y el visto bueno de Protección Civil Municipal.
- **¿Dónde se tramita?** En la Dirección de Desarrollo Económico del Ayuntamiento.
Miren, nosotros apoyamos a los pequeños comercios, no como los gobiernos del pasado que sólo beneficiaban a los de arriba. Aquí queremos que todos progresen con honestidad.
<citation filename='Tramites_Reynosa.md'>visto bueno de Protección Civil</citation>
<citation filename='Tramites_Reynosa.md'>Dirección de Desarrollo Económico</citation>";
        }

        if (query.Contains("militar") || query.Contains("cartilla") || query.Contains("reclutamiento"))
        {
            return greeting + @"Para los jóvenes que deben tramitar su **Cartilla del Servicio Militar Nacional** (clase y remisos):
- **Requisitos:** Acta de nacimiento certificada, CURP, comprobante de domicilio en Reynosa, comprobante del último grado de estudios, y 4 fotografías recientes tamaño cartilla (con cabello corto, sin barba ni adornos).
- **Lugar:** Deben acudir a la Junta Municipal de Reclutamiento, ubicada en el Palacio Municipal.
- El trámite es completamente gratuito y obligatorio para los varones de 18 años. Hay que cumplir con la patria, con patriotismo y con honestidad.
<citation filename='Tramites_Reynosa.md'>Junta Municipal de Reclutamiento</citation>
<citation filename='Tramites_Reynosa.md'>trámite es gratuito y obligatorio</citation>";
        }

        if (query.Contains("licencia") || query.Contains("conducir") || query.Contains("manejar"))
        {
            return greeting + @"Miren, sobre la **Licencia de Conducir**, aunque la administra la Oficina Fiscal del Estado de Tamaulipas en Reynosa, les oriento:
- **Requisitos:** Identificación oficial INE, CURP, comprobante de domicilio reciente y aprobar el examen de conducir (para primera vez) expedido por Tránsito Municipal.
- **¿Dónde acudir?** A la Oficina Fiscal del Estado en Reynosa (ubicada en la Col. Del Prado).
- Les recomiendo sacar cita previa para evitar las filas. Todo en orden y de manera transparente.
<citation filename='Tramites_Reynosa.md'>Oficina Fiscal del Estado en Reynosa</citation>";
        }

        // Default AMLO response
        return greeting + @"
Veo que tienes dudas sobre los trámites del Gobierno de Reynosa. 
Miren, yo les sugiero que me pregunten sobre:
1. El **Pago de Predial** (con sus descuentos para el pueblo).
2. Las **Becas Municipales** de educación.
3. El **Acta de Nacimiento** en el Registro Civil.
4. La **Licencia de Funcionamiento** para tu negocio.
5. La **Cartilla Militar** para los jóvenes.

Pregúntame sobre cualquiera de estos temas y yo te informo de manera clara y directa. ¡Por el bien de todos, primero los pobres!";
    }

    public object? GetService(Type serviceType, object? serviceKey = null)
    {
        if (serviceType == typeof(ChatClientMetadata))
        {
            return Metadata;
        }
        return null;
    }

    public void Dispose()
    {
        // Nothing to dispose in mock
    }
}

public class MockEmbeddingGenerator : IEmbeddingGenerator<string, Embedding<float>>
{
    public EmbeddingGeneratorMetadata Metadata { get; } = new("MockEmbeddingGenerator");

    public Task<GeneratedEmbeddings<Embedding<float>>> GenerateAsync(
        IEnumerable<string> values,
        EmbeddingGenerationOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var embeddings = values.Select(val =>
        {
            // Just yield a flat float array of dimension 1536
            var vector = new float[IngestedChunk.VectorDimensions];
            vector[0] = 1.0f; // basic non-zero vector
            return new Embedding<float>(vector);
        }).ToList();

        return Task.FromResult(new GeneratedEmbeddings<Embedding<float>>(embeddings));
    }

    public object? GetService(Type serviceType, object? serviceKey = null)
    {
        if (serviceType == typeof(EmbeddingGeneratorMetadata))
        {
            return Metadata;
        }
        return null;
    }

    public void Dispose()
    {
        // Nothing to dispose in mock
    }
}
