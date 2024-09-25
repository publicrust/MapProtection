import React, { useState, ChangeEvent, FormEvent } from "react";

interface MapProtectOptions {
  protectRustEditData: boolean;
  protectDeployables: boolean;
  protectAgainstEditors: boolean;
  uploadMap: boolean;
  entitySpam: number;
}

const MapProtectionForm: React.FC = () => {
  const [file, setFile] = useState<File | null>(null);
  const [options, setOptions] = useState<MapProtectOptions>({
    protectRustEditData: true,
    protectDeployables: true,
    protectAgainstEditors: true,
    uploadMap: true,
    entitySpam: 5000,
  });
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [submitMessage, setSubmitMessage] = useState("");
  const [retryCount, setRetryCount] = useState(0);

  const handleFileChange = (event: ChangeEvent<HTMLInputElement>) => {
    if (event.target.files) {
      setFile(event.target.files[0]);
    }
  };

  const handleOptionChange = (event: ChangeEvent<HTMLInputElement>) => {
    const { name, value, type, checked } = event.target;
    setOptions((prevOptions) => ({
      ...prevOptions,
      [name]: type === "checkbox" ? checked : parseInt(value),
    }));
  };

  const handleSubmit = async (event: FormEvent) => {
    event.preventDefault();
    if (!file) {
      setSubmitMessage("Please select a map file.");
      return;
    }
    setIsSubmitting(true);
    setSubmitMessage("");
    try {
      const formData = new FormData();
      formData.append("mapFile", file);
      formData.append("SpamAmount", options.entitySpam.toString());
      formData.append("IsRustEditDataProtectChecked", options.protectRustEditData.toString());
      formData.append("IsDeployProtectChecked", options.protectDeployables.toString());
      formData.append("IsEditProtectChecked", options.protectAgainstEditors.toString());
      formData.append("IsUploadMap", options.uploadMap.toString());

      const response = await fetch('https://localhost:7003/MapProtection/protect', {
        method: 'POST',
        body: formData,
      });

      if (!response.ok) {
        const errorData = await response.json().catch(() => ({}));
        throw new Error(errorData.message || 'Failed to protect the map file');
      }

      const contentType = response.headers.get("content-type");
      if (contentType && contentType.indexOf("application/octet-stream") !== -1) {
        const blob = await response.blob();
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.style.display = 'none';
        a.href = url;
        a.download = 'MapProtection.cs';
        document.body.appendChild(a);
        a.click();
        window.URL.revokeObjectURL(url);
        document.body.removeChild(a);

        setSubmitMessage("Map protection added successfully! The protected map file is being downloaded.");
      } else {
        throw new Error('The server did not return a file. Please try again.');
      }

      setRetryCount(0);
    } catch (error: unknown) {
      console.error("Error protecting map:", error);
      setSubmitMessage(`An error occurred while protecting the map: ${error instanceof Error ? error.message : 'Unknown error'}. Please try again.`);
      setRetryCount((prevCount) => prevCount + 1);
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleRetry = () => {
    if (retryCount < 3) {
      handleSubmit({ preventDefault: () => {} } as FormEvent);
    } else {
      setSubmitMessage("Maximum retry attempts reached. Please check your connection and try again later.");
    }
  };

  return (
    <div className="font-mono p-6 bg-white">
      <h1 className="text-3xl font-bold mb-6">MAP PROTECTION</h1>
      <form onSubmit={handleSubmit} className="space-y-4">
        <div>
          <label className="block mb-2">MapFile:</label>
          <input
            type="text"
            readOnly
            value={file ? file.name : 'No file selected'}
            className="w-full border border-gray-300 p-2 rounded"
          />
          <input
            type="file"
            onChange={handleFileChange}
            className="hidden"
            id="fileInput"
            accept=".map"
          />
          <label
            htmlFor="fileInput"
            className="inline-block mt-2 bg-gray-200 hover:bg-gray-300 px-4 py-2 rounded cursor-pointer"
          >
            CHOOSE MAP
          </label>
        </div>
        <div>
          <p className="font-bold mb-2">Protections:</p>
          <label className="flex items-center space-x-2">
            <input
              type="checkbox"
              name="protectRustEditData"
              checked={options.protectRustEditData}
              onChange={handleOptionChange}
              className="form-checkbox"
            />
            <span>Protect RustEditData (IO/Loot/NPCs/Paths)</span>
          </label>
          <label className="flex items-center space-x-2">
            <input
              type="checkbox"
              name="protectDeployables"
              checked={options.protectDeployables}
              onChange={handleOptionChange}
              className="form-checkbox"
            />
            <span>Protect Deployables, Spawners, Entities</span>
          </label>
          <label className="flex items-center space-x-2">
            <input
              type="checkbox"
              name="protectAgainstEditors"
              checked={options.protectAgainstEditors}
              onChange={handleOptionChange}
              className="form-checkbox"
            />
            <span>Protect Against Editors/Servers</span>
          </label>
          <label className="flex items-center space-x-2">
            <input
              type="checkbox"
              name="uploadMap"
              checked={options.uploadMap}
              onChange={handleOptionChange}
              className="form-checkbox"
            />
            <span>Upload map in Rust Hosting</span>
          </label>
        </div>
        <div>
          <label className="block mb-2">Entity Spam:</label>
          <input
            type="number"
            name="entitySpam"
            value={options.entitySpam}
            onChange={handleOptionChange}
            className="w-full border border-gray-300 p-2 rounded"
          />
        </div>
        <button
          type="submit"
          className="w-full bg-black text-white font-bold py-2 px-4 rounded hover:bg-gray-800 disabled:opacity-50 disabled:cursor-not-allowed"
          disabled={isSubmitting}
        >
          {isSubmitting ? "PROCESSING..." : "ADD PROTECTION"}
        </button>
        {submitMessage && (
          <div className="mt-2 text-center">
            <p className={`${submitMessage.includes("successfully") ? "text-green-600" : "text-red-600"}`}>
              {submitMessage}
            </p>
            {!submitMessage.includes("successfully") && retryCount < 3 && (
              <button
                onClick={handleRetry}
                className="mt-2 bg-blue-500 text-white font-bold py-2 px-4 rounded hover:bg-blue-600"
              >
                Retry
              </button>
            )}
          </div>
        )}
      </form>
    </div>
  );
};

export default MapProtectionForm;
