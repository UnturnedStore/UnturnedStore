using Markdig;
using Markdig.Parsers;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Website.Components.Helpers;

namespace Website.Components.MarkdownEditor
{
    public partial class MarkdownEditor
    {
        [Parameter]
        public string Value { get; set; }

        [Parameter]
        public EventCallback<string> ValueChanged { get; set; }

        [Parameter]
        public Expression<Func<string>> ValueExpression { get; set; }

        [Parameter]
        public bool EnableToolbar { get; set; } = true;

        [Parameter]
        public string id { get; set; }

        [Parameter]
        public bool DisableHtml { get; set; } = true;

        [Parameter]
        public string[] HighlightedPlaceholders { get; set; } = Array.Empty<string>();
        private bool HighlightPlaceholders => HighlightedPlaceholders.Length > 0;

        [CascadingParameter]
        private EditContext CascadedEditContext { get; set; }

        private bool isWriteActive = true;

        private string _previewText = "";

        [Parameter]
        public int Height { get; set; } = 500;
        private string heightString => Height + "px";

        private FieldIdentifier _fieldIdentifier;
        private string _fieldCssClasses => CascadedEditContext?.FieldCssClass(_fieldIdentifier) ?? "";

        protected override void OnInitialized()
        {
            _fieldIdentifier = FieldIdentifier.Create(ValueExpression);
        }

        protected override void OnParametersSet()
        {
            if (!string.IsNullOrEmpty(Value))
            {
                UpdatePreview();
            }            
        }

        private async Task HandleInput(ChangeEventArgs args)
        {
            await ValueChanged.InvokeAsync(args.Value.ToString());

            _previewText = ParsePlaceholders(MarkdownHelper.ParseToHtml(args.Value.ToString(), DisableHtml));

            CascadedEditContext?.NotifyFieldChanged(_fieldIdentifier);            
        }

        private void UpdatePreview()
        {
            _previewText = ParsePlaceholders(MarkdownHelper.ParseToHtml(Value.ToString(), DisableHtml));
        }

        private string ParsePlaceholders(string markdown)
        {
            if (!HighlightPlaceholders) return markdown;
            foreach (string placeholder in HighlightedPlaceholders)
                markdown = markdown.Replace($"<{placeholder}>", $"<span style=\"color: #fd7e14;\">{placeholder}</span>");
            return markdown;
        }

        private void HandleBoldClick()
        {
            Value = $"{Value} **(Bolded Text Here)**";
            UpdatePreview();
        }

        private void HandleItalicClick()
        {
            Value = $"{Value} *(Italic Text Here)*";
            UpdatePreview();
        }

        private void HandleListClick()
        {
            Value = $"{Value} \n - List Item";
            UpdatePreview();
        }

        private void HandlePlaceholderClick(string Placeholder)
        {
            Value = $"{Value} <{Placeholder}>";
            UpdatePreview();
        }

        private void HandleWriteClick()
        {
            isWriteActive = true;
        }

        private void HandlePreviewClick()
        {
            isWriteActive = false;
        }
    }
}
