using System;
using MedicalApp.Models;

namespace MedicalApp.Services
{
    public class SharedStateService : ISharedStateService
    {
        private Patient? _currentPatient;
        private readonly string _authFilePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "authenticated_doctors.txt");

        public string? ActiveDoctorName { get; set; }
        public System.Collections.Generic.HashSet<string> AuthenticatedDoctors { get; } = new(StringComparer.OrdinalIgnoreCase);

        public SharedStateService()
        {
            LoadAuthenticatedDoctors();
        }

        public bool IsDoctorAuthenticated(string doctorName)
        {
            if (string.IsNullOrWhiteSpace(doctorName)) return false;
            if (AuthenticatedDoctors.Contains(doctorName)) return true;
            LoadAuthenticatedDoctors();
            return AuthenticatedDoctors.Contains(doctorName);
        }

        public void AuthenticateDoctor(string doctorName)
        {
            if (string.IsNullOrWhiteSpace(doctorName)) return;
            LoadAuthenticatedDoctors();
            if (AuthenticatedDoctors.Add(doctorName))
            {
                SaveAuthenticatedDoctors();
            }
        }

        private void LoadAuthenticatedDoctors()
        {
            try
            {
                if (!System.IO.File.Exists(_authFilePath))
                {
                    AuthenticatedDoctors.Clear();
                    return;
                }

                var lines = System.IO.File.ReadAllLines(_authFilePath);
                if (lines.Length == 0)
                {
                    AuthenticatedDoctors.Clear();
                    return;
                }

                var dateStr = lines[0].Trim();
                if (dateStr == DateTime.Today.ToString("yyyy-MM-dd"))
                {
                    AuthenticatedDoctors.Clear();
                    for (int i = 1; i < lines.Length; i++)
                    {
                        var name = lines[i].Trim();
                        if (!string.IsNullOrEmpty(name))
                        {
                            AuthenticatedDoctors.Add(name);
                        }
                    }
                }
                else
                {
                    AuthenticatedDoctors.Clear();
                    System.IO.File.Delete(_authFilePath);
                }
            }
            catch
            {
                // Fallback to in-memory if file operations fail
            }
        }

        private void SaveAuthenticatedDoctors()
        {
            try
            {
                var lines = new System.Collections.Generic.List<string>();
                lines.Add(DateTime.Today.ToString("yyyy-MM-dd"));
                lines.AddRange(AuthenticatedDoctors);
                System.IO.File.WriteAllLines(_authFilePath, lines);
            }
            catch
            {
            }
        }

        public Patient? CurrentPatient
        {
            get => _currentPatient;
            set
            {
                if (_currentPatient != value)
                {
                    _currentPatient = value;
                    CurrentPatientChanged?.Invoke(_currentPatient);
                }
            }
        }

        public event Action<Patient?>? CurrentPatientChanged;
    }
}
