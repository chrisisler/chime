import { useIsMutating } from '@tanstack/react-query';
import { FC, useState } from 'react';

import { useCreateChime } from '../api';

export const CreateChime: FC = () => {
  const [chime, setChime] = useState('');

  const isMutating = useIsMutating() > 0;
  const createChime = useCreateChime();

  // const byId = `${navigator.platform}: ${navigator.userAgent}`;
  const byId = 9001; // TODO authentication
  const by = 'anonymous';
  const mediaUrl = null; // media support

  return (
    <div className="border-gray-600 rounded-md p-8 border space-y-4 mt-8">
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

      <button
        className="hover:cursor-pointer disabled:opacity-30 disabled:cursor-not-allowed"
        disabled={chime.length < 3 || isMutating}
        onClick={async () => {
          await createChime({ by, byId, text: chime, kids: [], mediaUrl });

          setChime('');
        }}
      >
        Post
      </button>
    </div>
  );
};
