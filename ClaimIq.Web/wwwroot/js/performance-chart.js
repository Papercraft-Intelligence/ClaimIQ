let performanceChart;

window.initializePerformanceChart = () => {
    const ctx = document.getElementById('performanceChart').getContext('2d');
    
    performanceChart = new Chart(ctx, {
        type: 'line',
        data: {
            labels: [],
            datasets: [{
                label: 'Response Time (ms)',
                data: [],
                borderColor: '#28a745',
                backgroundColor: 'rgba(40, 167, 69, 0.1)',
                borderWidth: 2,
                fill: true,
                tension: 0.4
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            scales: {
                y: {
                    beginAtZero: true,
                    title: {
                        display: true,
                        text: 'Response Time (ms)'
                    }
                },
                x: {
                    title: {
                        display: true,
                        text: 'Request Number'
                    }
                }
            },
            plugins: {
                title: {
                    display: true,
                    text: 'Redis Performance - Last 20 Requests'
                },
                legend: {
                    display: true
                }
            },
            animation: {
                duration: 750
            }
        }
    });
};

window.updatePerformanceChart = (responseTimes) => {
    if (!performanceChart) return;
    
    const labels = responseTimes.map((_, index) => `Req ${index + 1}`);
    
    performanceChart.data.labels = labels;
    performanceChart.data.datasets[0].data = responseTimes;
    
    // Color code based on performance
    const colors = responseTimes.map(time => {
        if (time < 20) return '#28a745'; // Green - excellent
        if (time < 50) return '#ffc107'; // Yellow - good  
        if (time < 100) return '#fd7e14'; // Orange - okay
        return '#dc3545'; // Red - slow
    });
    
    performanceChart.data.datasets[0].borderColor = colors[colors.length - 1];
    performanceChart.update();
};