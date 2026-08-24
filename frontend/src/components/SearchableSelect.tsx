import { useEffect, useMemo, useRef, useState } from 'react';

export interface SearchableSelectOption {
  id: number;
  label: string;
}

interface SearchableSelectProps {
  id: string;
  options: SearchableSelectOption[];
  value: number | '';
  onChange: (id: number | '') => void;
  placeholder: string;
  disabled?: boolean;
  maxResults?: number;
}

export function SearchableSelect({
  id,
  options,
  value,
  onChange,
  placeholder,
  disabled = false,
  maxResults = 50,
}: SearchableSelectProps) {
  const [query, setQuery] = useState('');
  const [isOpen, setIsOpen] = useState(false);
  const [highlightedIndex, setHighlightedIndex] = useState(0);
  const containerRef = useRef<HTMLDivElement>(null);

  const selectedLabel = useMemo(
    () => options.find((o) => o.id === value)?.label ?? '',
    [options, value],
  );

  // Keep the displayed text in sync with the selected value when the field isn't being edited.
  useEffect(() => {
    if (!isOpen) setQuery(selectedLabel);
  }, [selectedLabel, isOpen]);

  const filteredOptions = useMemo(() => {
    const trimmed = query.trim().toLowerCase();
    const matches = trimmed
      ? options.filter((o) => o.label.toLowerCase().includes(trimmed))
      : options;
    return matches.slice(0, maxResults);
  }, [options, query, maxResults]);

  useEffect(() => {
    if (isOpen) setHighlightedIndex(0);
  }, [filteredOptions, isOpen]);

  useEffect(() => {
    function handleClickOutside(event: MouseEvent) {
      if (containerRef.current && !containerRef.current.contains(event.target as Node)) {
        setIsOpen(false);
        setQuery(selectedLabel);
      }
    }
    document.addEventListener('mousedown', handleClickOutside);
    return () => document.removeEventListener('mousedown', handleClickOutside);
  }, [selectedLabel]);

  function selectOption(option: SearchableSelectOption) {
    onChange(option.id);
    setQuery(option.label);
    setIsOpen(false);
  }

  function handleKeyDown(e: React.KeyboardEvent<HTMLInputElement>) {
    if (!isOpen && (e.key === 'ArrowDown' || e.key === 'Enter')) {
      setIsOpen(true);
      return;
    }
    if (!isOpen) return;

    if (e.key === 'ArrowDown') {
      e.preventDefault();
      setHighlightedIndex((i) => Math.min(i + 1, filteredOptions.length - 1));
    } else if (e.key === 'ArrowUp') {
      e.preventDefault();
      setHighlightedIndex((i) => Math.max(i - 1, 0));
    } else if (e.key === 'Enter') {
      e.preventDefault();
      const option = filteredOptions[highlightedIndex];
      if (option) selectOption(option);
    } else if (e.key === 'Escape') {
      setIsOpen(false);
      setQuery(selectedLabel);
    }
  }

  const listboxId = `${id}-listbox`;

  return (
    <div className="searchable-select" ref={containerRef}>
      <input
        id={id}
        type="text"
        role="combobox"
        aria-expanded={isOpen}
        aria-controls={listboxId}
        aria-autocomplete="list"
        aria-activedescendant={isOpen && filteredOptions[highlightedIndex] ? `${id}-option-${filteredOptions[highlightedIndex].id}` : undefined}
        autoComplete="off"
        disabled={disabled}
        placeholder={placeholder}
        value={query}
        onFocus={() => setIsOpen(true)}
        onClick={() => setIsOpen(true)}
        onChange={(e) => {
          setQuery(e.target.value);
          setIsOpen(true);
          if (value !== '') onChange('');
        }}
        onKeyDown={handleKeyDown}
      />
      {isOpen && !disabled && (
        <ul className="searchable-select-list" role="listbox" id={listboxId}>
          {filteredOptions.length === 0 && <li className="searchable-select-empty">No matches</li>}
          {filteredOptions.map((option, index) => (
            <li
              key={option.id}
              id={`${id}-option-${option.id}`}
              role="option"
              aria-selected={option.id === value}
              className={index === highlightedIndex ? 'highlighted' : undefined}
              onMouseDown={(e) => {
                e.preventDefault();
                selectOption(option);
              }}
              onMouseEnter={() => setHighlightedIndex(index)}
            >
              {option.label}
            </li>
          ))}
          {options.length > filteredOptions.length && filteredOptions.length === maxResults && (
            <li className="searchable-select-hint">Keep typing to narrow results…</li>
          )}
        </ul>
      )}
    </div>
  );
}
