using System;

namespace MTurkAPIHelpers.Models
{
    public class BatchWorker
    {
        public int BatchId { get; set; }
        public string WorkerId { get; set; }
        public DateTime AssignmentDate { get; set; }
    }
}
