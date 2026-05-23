using O24OpenAPI.Core;
using O24OpenAPI.Core.Domain;
using O24OpenAPI.Core.Extensions;
using O24OpenAPI.Core.SeedWork;
using O24OpenAPI.Data.System.Linq;
using O24OpenAPI.Framework.Domain;
using O24OpenAPI.Framework.Models;
using System.Text.Json;

namespace O24OpenAPI.Framework.Services.Configuration;

public class CodeListService(IRepository<C_CODELIST> codeListRepository) : ICodeListService
{
    private readonly IRepository<C_CODELIST> _codeListRepository = codeListRepository;

    public async Task<IPagedList<C_CODELIST>> GetByGroupAndName(
        CodeListGroupAndNameRequestModel model
    )
    {
        var query = _codeListRepository.Table;

        if (model.CodeName.HasValue())
        {
            query = query.Where(s =>
                s.CodeName.Contains(model.CodeName, StringComparison.OrdinalIgnoreCase)
            );
        }
        if (model.CodeGroup.HasValue())
        {
            query = query.Where(s =>
                s.CodeGroup.Contains(model.CodeGroup, StringComparison.OrdinalIgnoreCase)
            );
        }
        var page = await query
            .Select(s => new
            {
                s.Id,
                s.CodeId,
                s.CodeGroup,
                s.CodeName,
                s.CodeValue,
                s.CodeIndex,
                s.Caption,
                s.MCaption,
                s.Ftag,
                s.Visible,
            })
            .ToPagedList(model.PageIndex, model.PageSize);
        var lang = model.Language ?? "en";

        var items = page.Select(s => new C_CODELIST
        {
            Id = s.Id,
            CodeId = s.CodeId,
            CodeGroup = s.CodeGroup ?? "",
            CodeName = s.CodeName ?? "",
            CodeValue = s.CodeValue ?? "",
            CodeIndex = s.CodeIndex,
            Ftag = s.Ftag ?? "",
            Visible = s.Visible,
            Caption = ResolveCaption(s.MCaption, s.Caption, lang),
        })
            .ToList();

        return new PagedList<C_CODELIST>(items, page.PageIndex, page.PageSize, page.TotalCount);
    }

    private static string ResolveCaption(
        string? mcaptionJson,
        string? fallbackCaption,
        string? language
    )
    {
        var lang = (language ?? "en").Trim().ToLowerInvariant();

        string[] langCandidates = lang switch
        {
            "vi" => ["vi", "vn"],
            "vn" => ["vn", "vi"],
            "lo" => ["lo", "la"],
            _ => [lang],
        };

        if (string.IsNullOrWhiteSpace(mcaptionJson))
        {
            return fallbackCaption ?? string.Empty;
        }

        try
        {
            var dict = JsonSerializer.Deserialize<Dictionary<string, string>>(mcaptionJson) ?? [];

            foreach (var key in langCandidates)
            {
                if (dict.TryGetValue(key, out var val) && !string.IsNullOrWhiteSpace(val))
                {
                    return val;
                }
            }

            if (dict.TryGetValue("en", out var enVal) && !string.IsNullOrWhiteSpace(enVal))
            {
                return enVal;
            }

            var any = dict.Values.FirstOrDefault(v => !string.IsNullOrWhiteSpace(v));
            if (!string.IsNullOrWhiteSpace(any))
            {
                return any;
            }

            return fallbackCaption ?? string.Empty;
        }
        catch
        {
            return fallbackCaption ?? string.Empty;
        }
    }

    public async Task<string> GetCaption(string codeId, string codeName, string codeGroup, string? language = null)
    {
        var lang = (language ?? "en").Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(codeId))
            return string.Empty;
        if (string.IsNullOrWhiteSpace(codeName))
            return string.Empty;
        if (string.IsNullOrWhiteSpace(codeGroup))
            return string.Empty;

        var caption = await _codeListRepository.Table
            .Where(x => x.CodeId == codeId && x.CodeName == codeName && x.CodeGroup == codeGroup)
            .Select(x => new { x.Caption, x.MCaption })
            .FirstOrDefaultAsync();

        if (caption == null)
            return string.Empty;

        return ResolveCaption(caption.MCaption, caption.Caption, lang);

    }
}
