import { useRef } from 'react';

interface ImageUploadProps {
  label: string;
  value: string | null | undefined;
  onChange: (url: string) => void;
}

/**
 * Shows an upload button. If a URL/data-URL is present, displays the image in a preview box.
 * For this implementation we use a file input and convert to a data URL for preview.
 * The actual URL stored/sent to the API should be set by the parent (e.g. after uploading to storage).
 */
export default function ImageUpload({ label, value, onChange }: ImageUploadProps) {
  const inputRef = useRef<HTMLInputElement>(null);

  function handleFileChange(e: React.ChangeEvent<HTMLInputElement>) {
    const file = e.target.files?.[0];
    if (!file) return;
    const reader = new FileReader();
    reader.onload = () => {
      if (typeof reader.result === 'string') {
        onChange(reader.result);
      }
    };
    reader.readAsDataURL(file);
  }

  return (
    <div className="space-y-2">
      <label className="block text-sm font-medium text-gray-700">{label}</label>
      <button
        type="button"
        onClick={() => inputRef.current?.click()}
        className="px-3 py-1.5 text-sm border border-gray-300 rounded hover:bg-gray-50"
      >
        {value ? 'Change Image' : 'Upload Image'}
      </button>
      <input
        ref={inputRef}
        type="file"
        accept="image/*"
        className="hidden"
        onChange={handleFileChange}
      />
      {value && (
        <div className="mt-2 border border-gray-200 rounded overflow-hidden w-40 h-40 flex items-center justify-center bg-gray-50">
          <img src={value} alt={label} className="object-contain w-full h-full" />
        </div>
      )}
    </div>
  );
}
