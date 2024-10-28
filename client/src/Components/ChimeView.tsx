import {
  faComment,
  faHeart,
  faMessage,
  faPaperclip,
} from '@fortawesome/free-solid-svg-icons';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { useIsMutating } from '@tanstack/react-query';
import { FC, useCallback, useRef, useState } from 'react';

import { formatUnix, useCreateComment } from '../api';
import { Chime } from '../interfaces';

export const ChimeView: FC<{ chime: Chime }> = ({ chime }) => {
  const [show, setShow] = useState(false);
  // for now
  const [liked, setLike] = useState(false);

  return (
    <div className="border-gray-600 rounded-md p-8 border space-y-10">
      <div className="flex space-x-4">
        <div className="h-12 w-12 rounded-full bg-gray-600" />

        <div className="-mt-1">
          <p className="font-bold text-lg">{chime.by}</p>
          <p className="text-gray-600">{formatUnix(chime.time)}</p>
        </div>
      </div>

      <p className="font-medium">{chime.text}</p>

      {chime.mediaUrl && (
        <img src={chime.mediaUrl} className="w-full rounded-md" alt="" />
      )}

      <div className="justify-between flex font-medium">
        <div
          className="flex space-x-3 items-center hover:cursor-pointer hover:scale-110"
          onClick={() => setLike(bool => !bool)}
        >
          <FontAwesomeIcon
            icon={faHeart}
            className={liked ? 'text-red-500' : ''}
          />
          <p>{Number(chime.text.length + (liked ? 1 : 0))}</p>
        </div>

        <div
          className="flex space-x-3 items-center cursor-pointer hover:scale-110"
          onClick={() => setShow(bool => !bool)}
        >
          <FontAwesomeIcon icon={faComment} />
          <p>{chime.kids.length}</p>
        </div>
      </div>

      {show && <Show chime={chime} />}

      <div className="w-full space-y-4">
        {chime.kids.map(commentId => (
          <p key={commentId} className="text-bold">
            {commentId}
          </p>
        ))}
      </div>
    </div>
  );
};

const Show: FC<{ chime: Chime }> = ({ chime }) => {
  const [comment, setComment] = useState('');

  const inputRef = useRef<null | HTMLInputElement>(null);

  const isMutating = useIsMutating() > 0;

  const by = "anonymous-commenter-dev";
  const byId = 0;

  const createComment = useCreateComment();

  const postComment = useCallback(async () => {
    await createComment([
      { by, byId, text: comment, parentId: chime.id },
      inputRef.current?.files?.[0]
    ]);

    setComment('');
    inputRef.current = null;

  }, [comment, by, byId, chime.id, inputRef]);

  const disabled = comment.length < 3 || isMutating;

  return (
    <div className="">
      <div className="flex space-x-3 w-full">
        <input
          value={comment}
          onChange={event => setComment(event.target.value)}
          className="rounded-lg p-4 w-full"
          placeholder="Add a comment..."
        />

        <button onClick={() => inputRef.current?.click()} disabled={disabled} className="hover:cursor-pointer">
          <FontAwesomeIcon
            icon={faPaperclip}
            className={disabled ? 'opacity-30' : ''}
          />
        </button>
        <input type="file" accept="image/*" ref={inputRef} className="hidden" />

        <button onClick={postComment} disabled={disabled}>
          <FontAwesomeIcon
            icon={faMessage}
            className={disabled ? 'opacity-30' : ''}
          />
        </button>
      </div>
    </div>
  );
};
