import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { faUpload } from '@fortawesome/free-solid-svg-icons';
import { useIsMutating, useMutation } from '@tanstack/react-query';
import { FC, useRef, useState } from 'react';

import { mutations } from '../api';

export const CreateChime: FC = () => {
  const inputRef = useRef<null | HTMLInputElement>(null);

  const [chime, setChime] = useState('');

  const isMutating = useIsMutating() > 0;

  const createItem = useMutation(mutations.createItem).mutateAsync;

  return (
    <div className="border-gray-600 rounded-md p-8 border space-y-4 mt-8">
      <input type="file" accept="image/*" ref={inputRef} className="hidden" />

      <p className="text-2xl font-extrabold mb-4">Create Chime</p>

      <textarea
        className="w-full rounded-md border border-gray-500 p-2 px-4"
        rows={3}
        maxLength={400}
        placeholder="What's on your mind?"
        value={chime}
        onChange={event => setChime(event.target.value)}
        disabled={isMutating}
      />

      <div className="space-x-4 flex items-center">
        <button
          className="hover:cursor-pointer disabled:opacity-30 disabled:cursor-not-allowed px-8"
          disabled={chime.length < 3 || isMutating}
          onClick={async () => {
            try {
              const f: File | undefined = inputRef.current?.files?.[0];

              await createItem([{ by: 'anonymous', byId: 9001, text: chime, parentId: null }, f]);

              setChime('');
              inputRef.current = null;
            } catch (err) {
              console.error('Failed to create chime: ' + err);
            }
          }}
        >
          Post
        </button>

        <button
          disabled={isMutating}
          onClick={() => {
            inputRef.current?.click();
          }}
          className="space-x-3 hover:cursor-pointer"
        >
          <FontAwesomeIcon
            icon={faUpload}
            className={chime.length < 3 || isMutating ? 'opacity-30' : ''}
          />
        </button>

        {!!inputRef.current && <p>{inputRef.current.name.slice(0, 50)}</p>}
      </div>

    </div>
  );
};
