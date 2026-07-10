using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using MedicalApp.Models;
using MedicalApp.Services;

namespace MedicalApp.Views
{
    public partial class StartNewVisitWindow : Window
    {
        private readonly IPatientService _patientService;

        public StartNewVisitWindow(IPatientService patientService)
        {
            InitializeComponent();
            _patientService = patientService;
            
            var viewModel = new StartNewVisitViewModel(_patientService);
            DataContext = viewModel;
        }

        private async void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is StartNewVisitViewModel vm)
            {
                await vm.LoadPatientsAsync();
            }
        }

        private async void SearchTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (DataContext is StartNewVisitViewModel vm)
                {
                    await vm.LoadPatientsAsync();
                }
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private async void StartVisitButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is StartNewVisitViewModel vm && vm.SelectedPatient != null)
            {
                decimal price = 0;
                if (vm.IsPaidVisit)
                {
                    decimal.TryParse(vm.VisitPrice, out price);
                }

                var visit = new Visit
                {
                    PatientId = vm.SelectedPatient.PatientId,
                    VisitDate = DateTime.UtcNow,
                    IsPaid = vm.IsPaidVisit,
                    VisitPrice = price
                };

                try
                {
                    await _patientService.AddVisitForCheckInAsync(visit);
                    DialogResult = true;
                    Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"خطأ أثناء بدء الزيارة: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }

    public class StartNewVisitViewModel : INotifyPropertyChanged
    {
        private readonly IPatientService _patientService;
        private string _searchTerm = string.Empty;
        private ObservableCollection<Patient> _patients = new();
        private Patient? _selectedPatient;
        private bool _isPaidVisit = true;
        private string _visitPrice = "25000";

        public event PropertyChangedEventHandler? PropertyChanged;

        public StartNewVisitViewModel(IPatientService patientService)
        {
            _patientService = patientService;
            _ = LoadPatientsAsync();
        }

        public string SearchTerm
        {
            get => _searchTerm;
            set
            {
                _searchTerm = value;
                OnPropertyChanged(nameof(SearchTerm));
                _ = LoadPatientsAsync();
            }
        }

        public ObservableCollection<Patient> Patients
        {
            get => _patients;
            set
            {
                _patients = value;
                OnPropertyChanged(nameof(Patients));
            }
        }

        public Patient? SelectedPatient
        {
            get => _selectedPatient;
            set
            {
                _selectedPatient = value;
                OnPropertyChanged(nameof(SelectedPatient));
            }
        }

        public bool IsPaidVisit
        {
            get => _isPaidVisit;
            set
            {
                _isPaidVisit = value;
                OnPropertyChanged(nameof(IsPaidVisit));
                OnPropertyChanged(nameof(IsFreeVisit));
            }
        }

        public bool IsFreeVisit
        {
            get => !_isPaidVisit;
            set => IsPaidVisit = !value;
        }

        public string VisitPrice
        {
            get => _visitPrice;
            set
            {
                _visitPrice = value;
                OnPropertyChanged(nameof(VisitPrice));
            }
        }

        public async Task LoadPatientsAsync()
        {
            try
            {
                var list = await _patientService.SearchPatientsAsync(SearchTerm);
                Patients = new ObservableCollection<Patient>(list);
            }
            catch
            {
                Patients = new ObservableCollection<Patient>();
            }
        }

        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
