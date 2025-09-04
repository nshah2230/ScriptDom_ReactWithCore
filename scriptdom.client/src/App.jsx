import { useEffect, useState } from 'react';
import './App.css';

function App() {
    const [sqlstatement, setSqlStatement] = useState();
    const [sqltabledata, setExtractedData] = useState();

    useEffect(() => {
        populateSQLData();
    }, []);
    
    const contents = sqltabledata === undefined
        ? <p>No data.</p>
        : <><p>{sqlstatement}</p>
        <table className="table table-striped" aria-labelledby="tableLabel">
            <thead>
                <tr>
                    <th>Table</th>
                    <th>Column Name</th>
                    <th>Column Type</th>
                </tr>
            </thead>
            <tbody>
                {sqltabledata.map((tabledata, index) =>
                    <tr key={index}>
                        <td key="t{index}">{tabledata.tableName}</td>
                        <td key="c{index}">{tabledata.columnName}</td>
                        <td key="ct{index}">{tabledata.columnType}</td>
                    </tr>
                )}
            </tbody>
            </table></>;

    return (
        <div>
            <h1 id="tableLabel">SQL Extract</h1>
            <p>This component demonstrates fetching data from the server.</p>
            {contents}
        </div>
    );
    
    async function populateSQLData() {
       
        const response = await fetch('dataextract');
        if (response.ok) {
            const data = await response.json();
            setSqlStatement(data[0].value.key)
            setExtractedData(data[0].value.value);
        }
    }
}

export default App;