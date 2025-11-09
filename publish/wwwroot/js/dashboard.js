class DashboardAr {
    constructor() {
        this.connection = new signalR.HubConnectionBuilder()
            .withUrl("/dashboardHub")
            .build();

        this.init();
    }

    async init() {
        await this.connectToHub();
        this.loadDashboardData();
        this.setupEventListeners();
        this.setupArabicUI();
    }

    setupArabicUI() {
   
        document.getElementById('loadingText').textContent = 'جاري تحميل البيانات...';
        document.getElementById('refreshBtn').textContent = 'تحديث البيانات';

        document.body.style.direction = 'rtl';
        document.body.style.textAlign = 'right';
    }

    async connectToHub() {
        try {
            await this.connection.start();
            await this.connection.invoke("SubscribeToDashboard");
            console.log("تم الاتصال بنجاح");
        } catch (err) {
            console.error("خطأ في الاتصال:", err);
            setTimeout(() => this.connectToHub(), 5000);
        }
    }

    async loadDashboardData() {
        try {
            this.showLoading();
            const response = await fetch('/api/dashboard/overview');
            const result = await response.json();

            if (result.success) {
                this.updateDashboardUI(result.data);
                this.renderCharts(result.data);
                this.hideLoading();
            } else {
                this.showError('فشل في تحميل البيانات');
            }
        } catch (error) {
            console.error('خطأ في تحميل بيانات اللوحة:', error);
            this.showError('حدث خطأ في تحميل البيانات');
        }
    }

    updateDashboardUI(data) {
 
        document.getElementById('totalResidents').textContent = this.formatNumber(data.totalResidents);
        document.getElementById('activeResidents').textContent = this.formatNumber(data.activeResidents);
        document.getElementById('vacantRooms').textContent = this.formatNumber(data.vacantRooms);
        document.getElementById('monthlyRevenue').textContent = this.formatCurrency(data.monthlyRevenue);
        document.getElementById('pendingPayments').textContent = this.formatCurrency(data.pendingPayments);

        document.querySelector('#residentsCard .card-title').textContent = 'إجمالي المقيمين';
        document.querySelector('#activeCard .card-title').textContent = 'المقيمين النشطين';
        document.querySelector('#vacantCard .card-title').textContent = 'الغرف الشاغرة';
        document.querySelector('#revenueCard .card-title').textContent = 'الإيراد الشهري';
        document.querySelector('#pendingCard .card-title').textContent = 'المدفوعات المتأخرة';
    }

    renderCharts(data) {
        this.renderRevenueChart(data.revenueTrend);
        this.renderOccupancyChart(data.occupancyStats);
        this.renderPaymentStatusChart(data);
    }

    renderRevenueChart(revenueData) {
        const ctx = document.getElementById('revenueChart').getContext('2d');

        new Chart(ctx, {
            type: 'line',
            data: {
                labels: revenueData.map(r => this.formatArabicDate(r.period)),
                datasets: [{
                    label: 'الإيراد الشهري',
                    data: revenueData.map(r => r.amount),
                    borderColor: '#007bff',
                    backgroundColor: 'rgba(0, 123, 255, 0.1)',
                    tension: 0.4,
                    fill: true
                }]
            },
            options: {
                responsive: true,
                plugins: {
                    legend: {
                        display: true,
                        position: 'top',
                        labels: {
                            font: {
                                family: 'Arial, sans-serif'
                            }
                        }
                    },
                    title: {
                        display: true,
                        text: 'تطور الإيرادات الشهرية',
                        font: {
                            size: 16,
                            family: 'Arial, sans-serif'
                        }
                    }
                },
                scales: {
                    y: {
                        beginAtZero: true,
                        ticks: {
                            callback: value => this.formatCurrency(value)
                        }
                    },
                    x: {
                        ticks: {
                            font: {
                                family: 'Arial, sans-serif'
                            }
                        }
                    }
                }
            }
        });
    }

    renderOccupancyChart(occupancyData) {
        const ctx = document.getElementById('occupancyChart').getContext('2d');

        new Chart(ctx, {
            type: 'bar',
            data: {
                labels: occupancyData.map(o => `نوع ${o.roomType}`),
                datasets: [
                    {
                        label: 'مشغولة',
                        data: occupancyData.map(o => o.occupied),
                        backgroundColor: '#28a745'
                    },
                    {
                        label: 'شاغرة',
                        data: occupancyData.map(o => o.vacant),
                        backgroundColor: '#dc3545'
                    }
                ]
            },
            options: {
                responsive: true,
                plugins: {
                    legend: {
                        display: true,
                        position: 'top',
                        labels: {
                            font: {
                                family: 'Arial, sans-serif'
                            }
                        }
                    },
                    title: {
                        display: true,
                        text: 'إحصاءات الإشغال',
                        font: {
                            size: 16,
                            family: 'Arial, sans-serif'
                        }
                    }
                },
                scales: {
                    x: {
                        stacked: true,
                        ticks: {
                            font: {
                                family: 'Arial, sans-serif'
                            }
                        }
                    },
                    y: {
                        stacked: true,
                        ticks: {
                            font: {
                                family: 'Arial, sans-serif'
                            }
                        }
                    }
                }
            }
        });
    }

    renderPaymentStatusChart(data) {
        const ctx = document.getElementById('paymentChart').getContext('2d');

        new Chart(ctx, {
            type: 'doughnut',
            data: {
                labels: ['مدفوع', 'متأخر', 'قيد الانتظار'],
                datasets: [{
                    data: [70, 20, 10], 
                    backgroundColor: ['#28a745', '#dc3545', '#ffc107']
                }]
            },
            options: {
                responsive: true,
                plugins: {
                    legend: {
                        position: 'bottom',
                        labels: {
                            font: {
                                family: 'Arial, sans-serif'
                            }
                        }
                    },
                    title: {
                        display: true,
                        text: 'حالة المدفوعات',
                        font: {
                            size: 16,
                            family: 'Arial, sans-serif'
                        }
                    }
                }
            }
        });
    }

    formatArabicDate(period) {
      
        const [year, month] = period.split('-');
        const months = {
            '01': 'يناير', '02': 'فبراير', '03': 'مارس', '04': 'أبريل',
            '05': 'مايو', '06': 'يونيو', '07': 'يوليو', '08': 'أغسطس',
            '09': 'سبتمبر', '10': 'أكتوبر', '11': 'نوفمبر', '12': 'ديسمبر'
        };
        return `${months[month]} ${year}`;
    }

    formatCurrency(amount) {
        return new Intl.NumberFormat('ar-SA', {
            style: 'currency',
            currency: 'SAR'
        }).format(amount);
    }

    formatNumber(number) {
        return new Intl.NumberFormat('ar-SA').format(number);
    }

    showLoading() {
        document.getElementById('loadingSpinner').classList.remove('d-none');
    }

    hideLoading() {
        document.getElementById('loadingSpinner').classList.add('d-none');
    }

    showError(message) {
     
        const errorDiv = document.getElementById('errorMessage');
        errorDiv.textContent = message;
        errorDiv.classList.remove('d-none');
        setTimeout(() => errorDiv.classList.add('d-none'), 5000);
    }

    setupEventListeners() {
     
        document.getElementById('refreshDashboard').addEventListener('click', () => {
            this.loadDashboardData();
        });

 
        setInterval(() => this.loadDashboardData(), 300000);

        this.connection.on("DashboardUpdated", (data) => {
            this.showNotification('تم تحديث البيانات تلقائياً');
            this.loadDashboardData();
        });
    }

    showNotification(message) {
  
        if ('Notification' in window && Notification.permission === 'granted') {
            new Notification('مغتربات هاوس', {
                body: message,
                icon: '/icon.png'
            });
        }
    }
}


document.addEventListener('DOMContentLoaded', () => {
    new DashboardAr();
});