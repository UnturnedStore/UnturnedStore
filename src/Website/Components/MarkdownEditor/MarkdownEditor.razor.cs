﻿using Markdig;
using Markdig.Parsers;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

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

        [CascadingParameter]
        private EditContext CascadedEditContext { get; set; }

        private bool isWriteActive = true;

        private string _previewText = "";

        private int height = 500;
        private string heightString => height + "px";

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

            _previewText = Markdown.ToHtml(args.Value.ToString());

            CascadedEditContext?.NotifyFieldChanged(_fieldIdentifier);            
        }

        private void UpdatePreview()
        {
            _previewText = Markdown.ToHtml(Value.ToString());
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
