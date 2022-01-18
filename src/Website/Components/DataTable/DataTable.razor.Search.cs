using Microsoft.AspNetCore.Components;
using System.Diagnostics;
using System.Timers;

namespace Website.Components.DataTable
{
    public partial class DataTable<TItem>
    {
        private string searchString = string.Empty;
        private System.Timers.Timer searchTimer;


        private bool isLoading = true;
        private void OnInputSearch(ChangeEventArgs args)
        {
            searchTimer.Stop();
            isLoading = true;
            searchString = args.Value.ToString();
            StateHasChanged();
            searchTimer.Start();
        }

        private void OnSearchTimeElapsed(object sender, ElapsedEventArgs e)
        {
            InvokeAsync(() => 
            {
                RefreshItems();                
                StateHasChanged();                
            });            
        }

        private double searchTime;

        public void ApplySearch(ref IEnumerable<TItem> data)
        {
            if (string.IsNullOrEmpty(searchString))
                return;

            Stopwatch watch = new();
            watch.Start();

            IEnumerable<DataTableColumn<TItem>> columns = Columns.Where(x => x.IsSearchable);

            data = data.Where(i => columns.Any(c => c.ValidateSearch(i, searchString)));

            watch.Stop();
            searchTime = watch.Elapsed.TotalMilliseconds;
        }

    }
}
