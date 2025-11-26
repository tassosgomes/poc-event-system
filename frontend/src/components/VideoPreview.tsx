const extractYoutubeId = (url: string) => {
  try {
    const parsed = new URL(url)
    if (parsed.hostname.includes('youtube.com')) {
      return parsed.searchParams.get('v')
    }

    if (parsed.hostname.includes('youtu.be')) {
      return parsed.pathname.replace('/', '')
    }
  } catch (err) {
    console.error('Failed to parse YouTube URL', err)
  }

  return null
}

interface Props {
  videoUrl?: string | null
}

export function VideoPreview({ videoUrl }: Props) {
  if (!videoUrl) {
    return <span>--</span>
  }

  const youtubeId = extractYoutubeId(videoUrl)
  if (youtubeId) {
    const embedUrl = `https://www.youtube.com/embed/${youtubeId}`
    return (
      <div className="video-preview">
        <iframe
          src={embedUrl}
          title="YouTube video player"
          allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture"
          allowFullScreen
        />
      </div>
    )
  }

  return (
    <a href={videoUrl} target="_blank" rel="noreferrer">
      Abrir link
    </a>
  )
}
