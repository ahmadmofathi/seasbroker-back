import type { RouteStopValue } from '../../api/types';
import FormSelect from './FormSelect';
import { getPortOptions } from '../../utils/portOptions';
import { stopLabel } from '../../utils/route';

interface RouteBuilderProps {
  stops: RouteStopValue[];
  onChange: (stops: RouteStopValue[]) => void;
  /** 'date' on the public form, 'datetime-local' in the admin. */
  etaType: 'date' | 'datetime-local';
  /** Earliest allowed ETA, in the same format as etaType. */
  minEta?: string;
  maxStops?: number;
  invalid?: boolean;
}

const EMPTY_STOP: RouteStopValue = { port: '', eta: '' };

const portSelectOptions = getPortOptions().map((p) => ({ value: p.value, text: p.label }));

/**
 * Builds a vessel route one port at a time: the first row is the vessel's next port, then
 * "Add port" appends the following call, and so on until the route is complete.
 */
const RouteBuilder: React.FC<RouteBuilderProps> = ({ stops, onChange, etaType, minEta, maxStops = 20, invalid }) => {
  const rows = stops.length > 0 ? stops : [EMPTY_STOP];

  const update = (index: number, patch: Partial<RouteStopValue>) => {
    onChange(rows.map((s, i) => (i === index ? { ...s, ...patch } : s)));
  };

  const remove = (index: number) => {
    onChange(rows.filter((_, i) => i !== index));
  };

  const last = rows[rows.length - 1];
  const canAdd = rows.length < maxStops && last.port !== '' && last.eta !== '';

  return (
    <div className={`route-builder${invalid ? ' is-invalid' : ''}`}>
      {rows.map((stop, index) => (
        // Keyed by port too: the port picker keeps its own search text, so a row whose port
        // changes (picked, or shifted up after a removal) remounts and shows the right port.
        <div className="row g-2 align-items-start mb-2" key={`${String(index)}-${stop.port}`}>
          <div className="col-md-7">
            <small className="text-muted d-block mb-1">{stopLabel(index)}</small>
            <FormSelect<{ port: string }>
              placeholder="Search ports..."
              options={portSelectOptions}
              formField="port"
              error=""
              formData={{ port: stop.port }}
              setFormData={(next) => {
                const value = typeof next === 'function' ? next({ port: stop.port }) : next;
                update(index, { port: value.port });
              }}
            />
          </div>
          <div className="col-md-4">
            <small className="text-muted d-block mb-1">ETA</small>
            <input
              type={etaType}
              className="form-control"
              aria-label={`${stopLabel(index)} ETA`}
              value={stop.eta}
              min={index > 0 && rows[index - 1].eta ? rows[index - 1].eta : minEta}
              onChange={(e) => { update(index, { eta: e.target.value }); }}
            />
          </div>
          <div className="col-md-1 text-end">
            {index > 0 && (
              <button
                type="button"
                className="btn-close mt-4"
                aria-label={`Remove ${stopLabel(index)}`}
                onClick={() => { remove(index); }}
              />
            )}
          </div>
        </div>
      ))}
      <button
        type="button"
        className="btn btn-sm btn-outline-primary"
        disabled={!canAdd}
        title={canAdd ? undefined : 'Pick a port and ETA for the last stop first'}
        onClick={() => { onChange([...rows, EMPTY_STOP]); }}
      >
        + Add port
      </button>
    </div>
  );
};

export default RouteBuilder;
