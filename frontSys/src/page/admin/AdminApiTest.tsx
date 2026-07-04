import { useState } from 'react';
import { useAdminAuth } from '../../context/AdminAuthContext';
import { runAllApiTests, HEALTH_CHECK_LABELS, type ApiTestResult } from '../../api/testRunner';

const AdminApiTest: React.FC = () => {
  const [results, setResults] = useState<ApiTestResult[]>([]);
  const [running, setRunning] = useState(false);
  const { isAuthenticated } = useAdminAuth();

  const runTests = async () => {
    setRunning(true);
    try {
      setResults(await runAllApiTests());
    } finally {
      setRunning(false);
    }
  };

  const passed = results.filter((r) => r.ok).length;
  const failed = results.filter((r) => !r.ok).length;

  return (
    <>
      {!isAuthenticated && (
        <div className="admin-warn-box">
          <i className="ri-information-line" /> Please sign in to run a full system health check.
        </div>
      )}

      <div className="admin-action-bar">
        <button type="button" className="admin-btn-sm primary" onClick={() => void runTests()} disabled={running}>
          <i className={running ? 'ri-loader-4-line' : 'ri-play-line'} />
          {running ? 'Checking services…' : 'Run System Health Check'}
        </button>
      </div>

      {results.length > 0 && (
        <>
          <div className="admin-test-summary">
            <span className="item pass">✓ {passed} passed</span>
            <span className="item fail">✗ {failed} failed</span>
            <span className="item" style={{ color: 'var(--admin-muted)' }}>{results.length} total</span>
          </div>

          <div className="admin-panel">
            <div className="admin-panel-body no-pad">
              <div className="admin-table-wrap">
                <table className="admin-table">
                  <thead>
                    <tr>
                      <th>Status</th>
                      <th>Service</th>
                      <th>Result</th>
                      <th>Time</th>
                    </tr>
                  </thead>
                  <tbody>
                    {results.map((r) => (
                      <tr key={r.id} className={`admin-test-row${r.ok ? ' pass' : ' fail'}`}>
                        <td>{r.ok ? '✅' : '❌'}</td>
                        <td style={{ fontWeight: 500, color: 'var(--admin-navy)' }}>
                          {HEALTH_CHECK_LABELS[r.id] ?? r.linkedPage}
                        </td>
                        <td style={{ fontSize: '0.8rem' }}>{r.message}</td>
                        <td style={{ color: '#94a3b8' }}>{r.durationMs}ms</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </div>
          </div>
        </>
      )}
    </>
  );
};

export default AdminApiTest;
