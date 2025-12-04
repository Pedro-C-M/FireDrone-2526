// Additional site-wide JavaScript functionality

// Initialize tooltips if using Bootstrap
document.addEventListener('DOMContentLoaded', function () {
    // You can add any additional initialization code here
    console.log('FireDrone Flight Plans Interface loaded');
});

// Helper function to show notifications
function showNotification(message, type = 'info') {
    // You can implement a toast notification system here
    console.log(`${type.toUpperCase()}: ${message}`);
}

// Auto-refresh flight plans every 30 seconds
setInterval(() => {
    if (typeof getFlightPlans === 'function') {
        getFlightPlans();
    }
}, 30000);
