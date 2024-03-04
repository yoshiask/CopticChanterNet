using CoptLib.Writing.Lexicon;
using System.Text;
using System.Xml.Linq;
using System.Xml;
using CoptLib.Models;
using CoptLib.Writing;
using CoptLib.Models.Text;

namespace CopticChanter.WebApi.Core.Responses;

[Serializable]
public record LexiconSearchResponse(string Query, List<LexiconEntry> Entries);

public static class LexiconSearchResponseReaderWriter
{
    public static LexiconSearchResponse FromXml(XDocument xml)
    {
        var xResponse = xml.Root!;

        var query = xResponse.Attribute("Query")?.Value ?? "";
        LexiconSearchResponse response = new(query, []);

        foreach (var xEntry in xResponse.Elements())
        {
            string id = xEntry.Attribute(nameof(LexiconEntry.Id))?.Value!;

            var typeStr = xEntry.Attribute(nameof(LexiconEntry.Type))?.Value!;
            var type = (EntryType)Enum.Parse(typeof(EntryType), typeStr);


            List<Form> forms = [];
            foreach (var xForm in xEntry.Element("Forms")?.Elements() ?? [])
            {
                var formTypeStr = xForm.Attribute(nameof(Form.Type))?.Value!;
                var formType = (FormType)Enum.Parse(typeof(FormType), formTypeStr);
                var usage = LanguageInfo.Parse(xForm.Attribute(nameof(Form.Usage))?.Value!);
                var orthography = xForm.Value;

                forms.Add(new(formType, usage, orthography));
            }

            List<Sense> senses = [];
            foreach (var xSense in xEntry.Element("Senses")?.Elements() ?? [])
            {
                var bibliography = xSense.Attribute(nameof(Sense.Bibliography))?.Value ?? "";

                TranslationCollection translations = [];
                foreach (var xTranslation in xSense.Elements())
                {
                    var translationLanguage = LanguageInfo.Parse(xTranslation.Attribute(nameof(IMultilingual.Language))?.Value!);
                    var translationText = xTranslation.Value;

                    var translation = new Run(translationText, null)
                    {
                        Language = translationLanguage,
                    };
                    translations.Add(translation);
                }

                senses.Add(new(translations, bibliography));
            }

            XElement xGrammarGroup = xEntry.Element("GrammarGroup");
            var partOfSpeechStr = xGrammarGroup.Attribute(nameof(GrammarGroup.PartOfSpeech))?.Value!;
            var partOfSpeech = (PartOfSpeech)Enum.Parse(typeof(PartOfSpeech), partOfSpeechStr);

            var numberStr = xGrammarGroup.Attribute(nameof(GrammarGroup.Number))?.Value!;
            var number = (Number)Enum.Parse(typeof(Number), numberStr);

            var genderStr = xGrammarGroup.Attribute(nameof(GrammarGroup.Gender))?.Value;
            Gender? gender = null;
            if (Enum.TryParse<Gender>(genderStr, out var parsedGender))
                gender = parsedGender;

            var subclass = xGrammarGroup.Attribute(nameof(GrammarGroup.Subclass))?.Value;
            var note = xGrammarGroup.Attribute(nameof(GrammarGroup.Note))?.Value;

            GrammarGroup grammarGroup = new(partOfSpeech, number, gender, [], subclass, note);
            foreach (var xGrammar in xGrammarGroup.Elements())
            {
                var grammarTypeStr = xGrammar.Attribute(nameof(GrammarEntry.Type))?.Value!;
                var grammarType = (GrammarType)Enum.Parse(typeof(GrammarType), grammarTypeStr);

                GrammarEntry grammar = new(grammarType, xGrammar.Value);
                grammarGroup.Entries!.Add(grammar);

            }

            LexiconEntry entry = new(id, type, forms, senses, grammarGroup);
            response.Entries.Add(entry);
        }

        return response;
    }

    public static XDocument ToXml(this LexiconSearchResponse response)
    {
        XElement xResponse = new("LexiconSearch");
        xResponse.SetAttributeValue("Query", response.Query);

        foreach (var entry in response.Entries)
        {
            XElement xEntry = new("Entry");
            
            xEntry.SetAttributeValue(nameof(entry.Type), entry.Type);
            xEntry.SetAttributeValue(nameof(entry.Id), entry.Id);

            XElement xForms = new("Forms");
            foreach (var form in entry.Forms)
            {
                XElement xForm = new("Form");
                xForm.SetAttributeValue(nameof(form.Type), form.Type);
                xForm.SetAttributeValue(nameof(form.Usage), form.Usage.ToString());
                xForm.SetValue(form.Orthography);
                xForms.Add(xForm);
            }
            xEntry.Add(xForms);

            XElement xSenses = new("Senses");
            foreach (var senses in entry.Senses)
            {
                XElement xSense = new("Sense");
                xSense.SetAttributeValue(nameof(senses.Bibliography), senses.Bibliography);

                foreach (var translation in senses.Translations)
                {
                    var translationText = translation.ToString();
                    if (string.IsNullOrWhiteSpace(translationText))
                        continue;

                    XElement xTranslation = new("Translation");
                    xTranslation.SetAttributeValue(nameof(translation.Language), translation.Language.ToString());
                    xTranslation.SetValue(translationText);
                    xSense.Add(xTranslation);
                }

                xSenses.Add(xSense);
            }
            xEntry.Add(xSenses);

            XElement xGrammarGroup = new("GrammarGroup");
            xGrammarGroup.SetAttributeValue(nameof(entry.GrammarGroup.PartOfSpeech), entry.GrammarGroup.PartOfSpeech);
            xGrammarGroup.SetAttributeValue(nameof(entry.GrammarGroup.Number), entry.GrammarGroup.Number);
            if (entry.GrammarGroup.Subclass is not null)
                xGrammarGroup.SetAttributeValue(nameof(entry.GrammarGroup.Subclass), entry.GrammarGroup.Subclass);
            if (entry.GrammarGroup.Note is not null)
                xGrammarGroup.SetAttributeValue(nameof(entry.GrammarGroup.Note), entry.GrammarGroup.Note);
            if (entry.GrammarGroup.Gender is not null)
                xGrammarGroup.SetAttributeValue(nameof(entry.GrammarGroup.Gender), entry.GrammarGroup.Gender);
            foreach (var grammar in entry.GrammarGroup.Entries ?? [])
            {
                XElement xGrammar = new("Grammar");
                xGrammar.SetAttributeValue(nameof(grammar.Type), grammar.Type.ToString());
                xGrammar.SetValue(grammar.Text);
                xGrammarGroup.Add(xGrammar);
            }
            xEntry.Add(xGrammarGroup);

            xResponse.Add(xEntry);
        }

        XDocument xDoc = new();
        xDoc.Add(xResponse);
        return xDoc;
    }

    public static Stream ToXmlString(this LexiconSearchResponse layout)
    {
        var xml = layout.ToXml();

        MemoryStream stream = new();
        StreamWriter streamWriter = new(stream, Encoding.Unicode);
        using var xmlWriter = XmlWriter.Create(streamWriter);
        xml.WriteTo(xmlWriter);
        xmlWriter.Flush();

        return stream;
    }

#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_0_OR_GREATER
    public static async Task<Stream> ToXmlStringAsync(this LexiconSearchResponse layout, CancellationToken token = default)
    {
        var xml = layout.ToXml();

        MemoryStream stream = new();
        StreamWriter streamWriter = new(stream, Encoding.Unicode);

#if NETCOREAPP2_0_OR_GREATER
        await
#endif
        using var xmlWriter = XmlWriter.Create(streamWriter, new XmlWriterSettings
        {
            Async = true,
        });
        await xml.WriteToAsync(xmlWriter, token);
        await xmlWriter.FlushAsync();

        stream.Position = 0;
        return stream;
    }
#endif
}
