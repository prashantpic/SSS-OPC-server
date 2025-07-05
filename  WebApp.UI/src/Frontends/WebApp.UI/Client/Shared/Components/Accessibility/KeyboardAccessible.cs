using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Opc.System.UI.Client.Shared.Components.Accessibility
{
    /// <summary>
    /// A base class for custom components to provide standardized keyboard accessibility,
    /// fulfilling requirements of WCAG 2.1 and REQ-UIX-001. Components deriving from this
    /// can easily implement common keyboard interactions like Enter/Space for activation
    /// and Escape for cancellation.
    /// </summary>
    public abstract class KeyboardAccessibleComponent : ComponentBase
    {
        /// <summary>
        /// An event callback that is invoked when a primary action is triggered by the user,
        /// typically by pressing the 'Enter' or 'Space' key.
        /// </summary>
        [Parameter]
        public EventCallback OnPrimaryAction { get; set; }

        /// <summary>
        /// An event callback that is invoked when a cancel action is triggered by the user,
        /// typically by pressing the 'Escape' key.
        /// </summary>
        [Parameter]
        public EventCallback OnCancelAction { get; set; }

        /// <summary>
        /// A protected method that derived components can call from their @onkeydown event handler.
        /// It processes the keyboard event and invokes the appropriate callback.
        /// </summary>
        /// <param name="e">The keyboard event arguments from the browser.</param>
        protected virtual async Task HandleKeyDownAsync(KeyboardEventArgs e)
        {
            switch (e.Key)
            {
                case "Enter":
                case " ": // Space key
                    if (OnPrimaryAction.HasDelegate)
                    {
                        await OnPrimaryAction.InvokeAsync();
                    }
                    break;

                case "Escape":
                    if (OnCancelAction.HasDelegate)
                    {
                        await OnCancelAction.InvokeAsync();
                    }
                    break;
            }
        }
    }
}