import { useState } from 'react';
import { Button } from '@/components/ui/Button';
import { Input } from '@/components/ui/Input';
import { subDays, subHours } from 'date-fns';

interface DateRangePickerProps {
  from: string;
  to: string;
  onChange: (from: string, to: string) => void;
}

const presets = [
  { label: '24h', getRange: () => ({ from: subHours(new Date(), 24), to: new Date() }) },
  { label: '7d', getRange: () => ({ from: subDays(new Date(), 7), to: new Date() }) },
  { label: '30d', getRange: () => ({ from: subDays(new Date(), 30), to: new Date() }) },
];

// Convert an ISO UTC string to a local datetime-local input value
function toLocalInput(iso: string): string {
  const d = new Date(iso);
  const offset = d.getTimezoneOffset();
  const local = new Date(d.getTime() - offset * 60000);
  return local.toISOString().slice(0, 16);
}

// Convert a datetime-local input value (local time) to an ISO UTC string
function fromLocalInput(value: string): string {
  return new Date(value).toISOString();
}

export default function DateRangePicker({ from, to, onChange }: DateRangePickerProps) {
  const [customFrom, setCustomFrom] = useState(from);
  const [customTo, setCustomTo] = useState(to);

  return (
    <div className="flex flex-wrap items-center gap-2">
      {presets.map((preset) => (
        <Button
          key={preset.label}
          variant="outline"
          size="sm"
          onClick={() => {
            const range = preset.getRange();
            const f = range.from.toISOString();
            const t = range.to.toISOString();
            onChange(f, t);
            setCustomFrom(f);
            setCustomTo(t);
          }}
        >
          {preset.label}
        </Button>
      ))}
      <Input
        type="datetime-local"
        value={toLocalInput(customFrom)}
        onChange={(e) => {
          const iso = fromLocalInput(e.target.value);
          setCustomFrom(iso);
          onChange(iso, customTo);
        }}
        className="w-44"
      />
      <span className="text-muted-foreground">to</span>
      <Input
        type="datetime-local"
        value={toLocalInput(customTo)}
        onChange={(e) => {
          const iso = fromLocalInput(e.target.value);
          setCustomTo(iso);
          onChange(customFrom, iso);
        }}
        className="w-44"
      />
    </div>
  );
}
