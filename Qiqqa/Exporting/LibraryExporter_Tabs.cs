﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Qiqqa.DocumentLibrary;
using Qiqqa.DocumentLibrary.WebLibraryStuff;
using Qiqqa.Documents.PDF;
using Utilities;
using Utilities.Collections;
using Utilities.Misc;
using Directory = Alphaleonis.Win32.Filesystem.Directory;
using File = Alphaleonis.Win32.Filesystem.File;
using Path = Alphaleonis.Win32.Filesystem.Path;


namespace Qiqqa.Exporting
{
    internal class LibraryExporter_Tabs
    {
        internal static void Export(WebLibraryDetail web_library_detail, List<PDFDocument> pdf_documents, string base_path, Dictionary<string, PDFDocumentExportItem> pdf_document_export_items)
        {
            Logging.Info("Exporting entries to TAB separated");

            // Write out the header
            DateTime now = DateTime.Now;
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("% -------------------------------------------------------------------------");
            sb.AppendLine(String.Format("% This tab separated file was generated by Qiqqa ({0}?ref=EXPTAB)", Common.Configuration.WebsiteAccess.Url_Documentation4Qiqqa));
            sb.AppendLine(String.Format("% {0} {1}", now.ToLongDateString(), now.ToLongTimeString()));
            sb.AppendLine("% Version 1");
            sb.AppendLine("% -------------------------------------------------------------------------");
            sb.AppendLine();

            // Headers
            sb.AppendFormat(
                "{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}\t{8}\t{9}\t{10}\t{11}\t{12}\t{13}\t{14}\t{15}\t{16}\t{17}\t{18}\t{19}\t{20}",
                "Fingerprint",
                "BibTexKey",
                "Year",
                "Title",
                "Authors",
                "Publication",
                "Rating",
                "ReadingStage",
                "IsFavourite",
                "IsVanillaReference",
                "Tags",
                "AutoTags",
                "DateAddedToDatabase",
                "DateLastRead",
                "DateLastModified",
                "Filename",
                "Comments",
                "Abstract",
                "",
                "",
                ""
                );
            sb.AppendLine();


            // Write out the entries
            for (int i = 0; i < pdf_documents.Count; ++i)
            {
                PDFDocument pdf_document = pdf_documents[i];

                try
                {
                    StatusManager.Instance.UpdateStatus("TabExport", String.Format("Exporting entry {0} of {1}", i, pdf_documents.Count), i, pdf_documents.Count);

                    HashSet<string> autotags_set = pdf_document.LibraryRef.Xlibrary.AITagManager.AITags.GetTagsWithDocument(pdf_document.Fingerprint);
                    string autotags = ArrayFormatter.ListElements(autotags_set.ToList(), ";");

                    sb.AppendFormat(
                        "{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}\t{8}\t{9}\t{10}\t{11}\t{12}\t{13}\t{14}\t{15}\t{16}\t{17}\t{18}\t{19}\t{20}",
                        pdf_document.Fingerprint,
                        pdf_document.BibTexKey,
                        pdf_document.YearCombined,
                        pdf_document.TitleCombined,
                        pdf_document.AuthorsCombined,
                        pdf_document.Publication,
                        pdf_document.Rating,
                        pdf_document.ReadingStage,
                        pdf_document.IsFavourite,
                        pdf_document.IsVanillaReference,
                        pdf_document.Tags,
                        autotags,
                        FormatDate(pdf_document.DateAddedToDatabase),
                        FormatDate(pdf_document.DateLastRead),
                        FormatDate(pdf_document.DateLastModified),
                        pdf_document_export_items.ContainsKey(pdf_document.Fingerprint) ? pdf_document_export_items[pdf_document.Fingerprint].filename : "",
                        FormatFreeText(pdf_document.Comments),
                        FormatFreeText(pdf_document.Abstract),
                        null,
                        null,
                        null
                        );

                    sb.AppendLine();
                }

                catch (Exception ex)
                {
                    Logging.Error(ex, "There was a problem exporting the tab representation for " + pdf_document);
                }
            }

            // Write to disk
            string filename = Path.GetFullPath(Path.Combine(base_path, @"Qiqqa.tab"));
            File.WriteAllText(filename, sb.ToString());

            StatusManager.Instance.UpdateStatus("TabExport", String.Format("Exported your tab entries to {0}", filename));
        }

        private static object FormatFreeText(string p)
        {
            if (String.IsNullOrEmpty(p))
            {
                return "";
            }

            return p.Replace("\t", "    ").Replace("\r\n", "    ").Replace("\n", "    ").Replace("\r", "    ");
        }

        private static string FormatDate(DateTime? date_time_nullable)
        {
            if (date_time_nullable.HasValue)
            {
                return date_time_nullable.Value.ToLongDateString();
            }
            else
            {
                return "";
            }
        }
    }
}
