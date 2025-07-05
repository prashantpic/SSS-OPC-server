// This file provides JavaScript interop functions for creating and managing charts.
window.chartInterop = {
    // A dictionary to store active chart instances, keyed by element ID.
    instances: {},

    /**
     * Creates a new Chart.js chart instance.
     * If a chart already exists for the given elementId, it is destroyed first.
     * @param {string} elementId The ID of the canvas element to render the chart in.
     * @param {string} chartType The type of chart to create (e.g., 'line', 'bar').
     * @param {object} data The data object for the chart.
     * @param {object} options The options object for the chart.
     */
    createChart: function (elementId, chartType, data, options) {
        if (this.instances[elementId]) {
            this.destroyChart(elementId);
        }

        const canvas = document.getElementById(elementId);
        if (!canvas) {
            console.error(`Chart.js Interop: Element with ID '${elementId}' not found.`);
            return;
        }

        const ctx = canvas.getContext('2d');
        if (!ctx) {
            console.error(`Chart.js Interop: Could not get 2D context for element '${elementId}'.`);
            return;
        }

        try {
            const newChart = new Chart(ctx, {
                type: chartType,
                data: data,
                options: options
            });
            this.instances[elementId] = newChart;
        } catch (e) {
            console.error("Error creating chart:", e);
        }
    },

    /**
     * Updates an existing chart with new data.
     * @param {string} elementId The ID of the canvas element holding the chart.
     * @param {object} newData The new data object to apply to the chart.
     */
    updateChart: function (elementId, newData) {
        const chart = this.instances[elementId];
        if (chart) {
            chart.data = newData;
            chart.update();
        } else {
            console.warn(`Chart.js Interop: Chart with ID '${elementId}' not found for update.`);
        }
    },

    /**
     * Destroys a chart instance to free up resources and prevent memory leaks.
     * @param {string} elementId The ID of the canvas element holding the chart to destroy.
     */
    destroyChart: function (elementId) {
        const chart = this.instances[elementId];
        if (chart) {
            chart.destroy();
            delete this.instances[elementId];
        }
    }
};