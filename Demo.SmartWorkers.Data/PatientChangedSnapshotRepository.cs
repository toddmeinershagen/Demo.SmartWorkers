﻿using Demo.SmartWorkers.Core;
using Demo.SmartWorkers.Core.Data;
using MongoDB.Driver;

namespace Demo.SmartWorkers.Data
{
    public class PatientChangedSnapshotRepository : BaseRepository, IPatientChangedSnapshotRepository
    {
        public bool Insert(PatientChangedSnapshot snapshot)
        {
            var database = GetDatabase();
            var patientChangedSnapshots = database.GetCollection<PatientChangedSnapshot>("patientChangedSnapshots");

            var result = patientChangedSnapshots.Insert(snapshot);

            return WasSuccessful(result);
        }

        private static bool WasSuccessful(WriteConcernResult result)
        {
            return !result.HasLastErrorMessage;
        }
    }
}