using System;
using System.Collections.Generic;

public interface IDataEntry {
    string Name { get; set; }
    List<InformationData> RetrieveData(Action refresh);
}
