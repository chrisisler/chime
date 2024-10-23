import { faComment, faHeart } from "@fortawesome/free-solid-svg-icons";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { FC, useState } from "react";

import { Chime } from "../interfaces"
import { formatUnix } from "../api";

export const ChimeView: FC<{ chime: Chime }> = ({ chime }) => {
  // for now
  const [liked, setLike] = useState(false);

  return (
    <div className="border-gray-600 rounded-md p-8 border space-y-6">
      <div className="flex space-x-4">
        <div className="h-12 w-12 rounded-full bg-gray-600" />

        <div className="-mt-1">
          <p className="font-bold text-lg">{chime.by}</p>
          <p className="text-gray-600">{formatUnix(chime.time)}</p>
        </div>
      </div>

      <p className="font-medium">{chime.text}</p>

      <div className="justify-between flex font-medium">
        <div className="flex space-x-3 items-center" onClick={() => setLike(b => !!b)}>
          <FontAwesomeIcon icon={faHeart} />
          <p>{chime.text.length + (liked ? 1 : 0)}</p>
        </div>

        <div className="flex space-x-3 items-center">
          <FontAwesomeIcon icon={faComment} />
          <p>{chime.kids.length}</p>
        </div>
      </div>
    </div>
  );
};
