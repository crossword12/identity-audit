import { useEffect, useState } from "react";
import { getApiHealth } from "./api/healthApi";
import "./App.css";

function App() {
  const [apiStatus, setApiStatus] = useState("Vérification...");
  const [isConnected, setIsConnected] = useState(false);

  useEffect(() => {
    async function checkApi() {
      try {
        const result = await getApiHealth();
        setApiStatus(result.status);
        setIsConnected(true);
      } catch {
        setApiStatus("API indisponible");
        setIsConnected(false);
      }
    }

    void checkApi();
  }, []);

  return (
    <main>
      <h1>Identity Audit</h1>
      <p>Outil d’audit des identités et des privilèges</p>

      <section>
        <h2>État du système</h2>

        <p>
          API ASP.NET Core :{" "}
          <strong style={{ color: isConnected ? "#22c55e" : "#ef4444" }}>
            {apiStatus}
          </strong>
        </p>
      </section>
    </main>
  );
}

export default App;
