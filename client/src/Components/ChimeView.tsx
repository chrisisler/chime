import {
  faComment,
  faHeart,
  faMessage,
  faPaperclip,
} from '@fortawesome/free-solid-svg-icons';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { useIsMutating } from '@tanstack/react-query';
import { FC, useCallback, useRef, useState } from 'react';

import { formatUnix, useCreateItem, useItems, } from '../api';
import { Chime, Comment } from '../interfaces';
import { St8, St8View } from '../St8';
import { Loading } from './Loading';

export const ChimeView: FC<{ chime: Chime }> = ({ chime }) => {
  const [show, setShow] = useState(false);
  // for now
  const [liked, setLike] = useState(false);

  const comments = St8.map(
    useItems(),
    items => items.filter((_): _ is Comment => _.type == "comment" && chime.kids.includes(_.id))
  );

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
        <img src={chime.mediaUrl} className="w-full rounded-md" alt="chime image" />
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
          <p>{Math.floor(chime.text.length/2 + (liked ? 1 : 0))}</p>
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

      <St8View data={comments} loading={() => <Loading />} error={() => null}>
        {comments => comments.length === 0 ? null : (
          <div className="w-full space-y-4">
            {comments.map(comment => (
              <div key={comment.time} >
                <div className="justify-between flex">
                  <p className="text-bold text-gray-500">{comment.by}</p>

                  <p className="self-end text-sm whitespace-nowrap text-gray-500">
                    {formatUnix(comment.time, true)}
                  </p>
                </div>
                <p>{comment.text}</p>

                {comment.mediaUrl && (
                  <img src={comment.mediaUrl} className="m-0 p-0 w-full rounded-md" alt="comment image" />
                )}
              </div>
            ))}
          </div>
        )}
      </St8View>
    </div>
  );
};

const Show: FC<{ chime: Chime }> = ({ chime }) => {
  const [comment, setComment] = useState('');

  const inputRef = useRef<null | HTMLInputElement>(null);

  const isMutating = useIsMutating() > 0;

  const by = 'anonymous-commenter-dev';
  const byId = 0;

  const createItem = useCreateItem();

  const postComment = useCallback(async () => {
    await createItem([
      { by, byId, text: comment, parentId: chime.id },
      inputRef.current?.files?.[0],
    ]);

    setComment('');
    inputRef.current = null;
  }, [comment, by, byId, chime.id, inputRef]);

  const disabled = comment.length < 3 || isMutating;

  return (
    <div className="flex space-x-3 w-full">
      <input
        autoFocus
        value={comment}
        onChange={event => setComment(event.target.value)}
        className="rounded-lg p-4 w-full"
        placeholder="Add a comment..."
      />

      <button onClick={postComment} disabled={disabled}>
        <FontAwesomeIcon
          icon={faMessage}
          className={disabled ? 'opacity-30' : ''}
        />
      </button>

      {/** delete button goes somewhere */}

      <input type="file" accept="image/*" ref={inputRef} className="hidden" />
      <button
        onClick={() => inputRef.current?.click()}
        disabled={disabled}
        className="hover:cursor-pointer"
      >
        <FontAwesomeIcon
          icon={faPaperclip}
          className={disabled ? 'opacity-30' : ''}
        />
      </button>
    </div>
  );
};
