﻿using CoptLib.IO;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace CopticChanterNet.Controllers
{
    public class TasbehaOrgController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Convert([FromForm] string html, [FromForm] int lyricId)
        {
            // Parse HTML into document tree
            var doc = TasbehaOrg.ConvertLyricsPage(html, lyricId);

            // Serialize document as XML to a stream
            // FIXME: Workaround for DocWriter.WriteDocXml closing the underlying stream.
            XDocument xDocument = DocWriter.SerializeDocXml(doc);
            MemoryStream stream = new();
            using XmlWriter xmlTextWriter = XmlWriter.Create(stream, new()
            {
                CloseOutput = false,
                Encoding = Encoding.Unicode,
                Indent = true,
                IndentChars = "    ",
            });
            xDocument.Save(xmlTextWriter);
            xmlTextWriter.Flush();
            stream.Position = 0;

            return File(stream, @"application/xml", $"TasbehaOrg_{lyricId}.xml");
        }
    }
}