using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Website.Components.Alerts
{
    public class AlertService
    {
        private List<AlertBox> alertBoxes = new List<AlertBox>();

        internal void AddAlertBox(AlertBox box)
        {
            alertBoxes.RemoveAll(x => x.ID == box.ID);
            alertBoxes.Add(box);
        }

        public void ShowAlert(string boxId, string text, AlertType alertType = AlertType.Primary)
        {
            AlertBox box = alertBoxes.FirstOrDefault(x => x.ID == boxId);

            if (box == null)
            {
                Console.WriteLine($"Box with ID {boxId} now found");
                return;
            }

            AlertBox hideBox = alertBoxes.FirstOrDefault(x => x.Group == box.Group && x.IsShow);
            if (hideBox != null)
            {
                hideBox.Hide();
            }

            box.Show(text, alertType);
        }

        public void HideAlert(string boxId) 
        {
            AlertBox box = alertBoxes.FirstOrDefault(x => x.ID == boxId);

            if (box == null)
            {
                Console.WriteLine($"Box with ID {boxId} now found");
                return;
            }

            box.Hide();
        }
    }
}
