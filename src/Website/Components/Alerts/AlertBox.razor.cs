using Microsoft.AspNetCore.Components;

namespace Website.Components.Alerts
{
    public partial class AlertBox
    {
        [Parameter]
        public string ID { get; set; }
        [Parameter]
        public string Group { get; set; }

        [Inject]
        public AlertService AlertService { get; set; }

        protected override void OnAfterRender(bool firstRender)
        {
            if (firstRender)
                AlertService.AddAlertBox(this);
        }

        public bool IsShow { get; set; }
        public MarkupString Text { get; set; }
        public AlertType Type { get; set; }

        public void Show(string text, AlertType alertType)
        {
            Type = alertType;
            Text = new MarkupString(text);
            IsShow = true;
            StateHasChanged();
        }

        public void Hide()
        {
            IsShow = false;
            StateHasChanged();
        }

        private string GetBackgroundClass()
        {
            return Type switch
            {
                AlertType.Primary => "bg-primary",
                AlertType.Warning => "bg-warning",
                AlertType.Danger => "bg-danger",
                AlertType.Info => "bg-info",
                AlertType.Success => "bg-success",
                _ => null,
            };
        }

        private string GetIcon()
        {
            return Type switch
            {
                AlertType.Primary => "fas fa-info",
                AlertType.Warning => "fas fa-exclamation-triangle",
                AlertType.Danger => "fas fa-exclamation-circle",
                AlertType.Info => "fas fa-info-circle",
                AlertType.Success => "fas fa-check-circle",
                _ => null,
            };
        }
    }
}
